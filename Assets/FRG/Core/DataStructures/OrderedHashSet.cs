// Ordered HashSet implementation by Free Range Games
// From Microsoft corefix
// https://github.com/dotnet/corefx/blob/master/src/System.Collections/src/System/Collections/Generic/HashSet.cs

// .NET HashSet license notice:

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace FRG.Core
{
    /// <summary>
    /// A HashSet that also preserves insertion order.
    /// </summary>
    /// <remarks>
    /// 1) Larger memory footprint than .NET HashSet and mono's HashSet (8 bytes per entry).
    /// 
    /// 2) Memory allocations are just about optimal. Sort() allocates, but could be optimized to remove the allocation.
    /// 
    /// 3) Certain normally memory-hungry operations like IsSubset() will not allocate at all
    /// against another OrderedHashSet nor an empty <see cref="ICollection{T}"/>.
    /// Against an <see cref="IList{T}">, it will only allocate on very large sets
    /// (currently those greater than 3200 entries).
    /// 
    /// 4) Any modification that adds new items may trigger an O(n) operation, but that is rare.
    /// 
    /// 5) Move, Insert and the assigning [] operator can devolve into triggering the O(n) operations frequently,
    /// similar to how <see cref="List{T}"/> will have terrible performance when inserting at the front.
    /// 
    /// 6) Remove, IndexOf, Move, Insert and the assigning [] operator are
    /// always O(log(n)) instead of a O(1) hashtable algorithm, though
    /// the log(n) part is a very cheap binary search of integers (see <see cref="FindOrderedIndex"/>).
    /// Keep in mind the O(1) requires a good hash algorithm.
    /// </remarks>
    [DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "By design")]
    public class OrderedHashSet<T> : IList<T>, ISet<T>, IList, ICapacity//, IPoolInitializable, IPoolInitializable<int>
    {
        // store lower 31 bits of hash code
        private const int Lower31BitMask = 0x7FFFFFFF;
        // when constructing a hashset from an existing collection, it may contain duplicates, 
        // so this is used as the max acceptable excess ratio of capacity to count. Note that
        // this is only used on the ctor and not to automatically shrink if the hashset has, e.g,
        // a lot of adds followed by removes. Users must explicitly shrink by calling TrimExcess.
        // This is set to 3 because capacity is acceptable as 2x rounded up to nearest prime.
        private const int ShrinkThreshold = 3;

        /// <summary>
        /// 2^MaxInsertionNumberShift is the maximum spacing between added insertion numbers.
        /// </summary>
        private const int MaxInsertionNumberShift = 12;

        /// <summary>
        /// Minimum new insertion number, so we can insert at the beginning a bunch.
        /// </summary>
        private const int MinInsertionNumber = int.MinValue + (1 << MaxInsertionNumberShift);

        [NonSerialized]
        private int[] _buckets;
        [NonSerialized]
        private Slot[] _slots;
        [NonSerialized]
        private int[] _orderedList;
        [NonSerialized]
        private int _count;
        [NonSerialized]
        private int _lastIndex;
        [NonSerialized]
        private int _freeList;
        [NonSerialized]
        private IEqualityComparer<T> _comparer;
        [NonSerialized]
        private int _version;

        #region Constructors

        public OrderedHashSet()
            : this(0, null)
        {
        }

        public OrderedHashSet(IEqualityComparer<T> comparer)
            : this(0, comparer)
        {
        }

        public OrderedHashSet(int capacity)
            : this(capacity, null)
        {
        }

        public OrderedHashSet(int capacity, IEqualityComparer<T> comparer)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", capacity, "Capacity must be nonnegative.");
            
            if (comparer == null)
            {
                comparer = SafeEqualityComparer<T>.Default;
            }

            _buckets = ArrayUtil.Empty<int>();
            _slots = ArrayUtil.Empty<Slot>();
            _orderedList = ArrayUtil.Empty<int>();
            _lastIndex = 0;
            _count = 0;
            _freeList = -1;
            _comparer = comparer;
            _version = 0;

            if (capacity > 0)
            {
                EnsureCapacity(capacity);
            }

            AssertInvariants();
        }

        public OrderedHashSet(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        /// <summary>
        /// Implementation Notes:
        /// Since resizes are relatively expensive (require rehashing), this attempts to minimize 
        /// the need to resize by setting the initial capacity based on size of collection. 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public OrderedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(0, comparer)
        {
            if (collection == null) { throw new ArgumentNullException("collection"); }
            
            this.UnionWith(collection);

            if (_slots.Length > ShrinkThreshold * _count)
            {
                TrimExcess();
            }
        }

        //void IPoolInitializable.PoolInitialize()
        //{
        //    EnsureCapacity(RecyclingPool.MaxCollectionCapacity);
        //}

        //void IPoolInitializable<int>.PoolInitialize(int capacity)
        //{
        //    EnsureCapacity(Math.Max(RecyclingPool.MaxCollectionCapacity, capacity));
        //}

        #endregion

        #region ICollection<T> methods

        /// <summary>
        /// Add item to this hashset. This is the explicit implementation of the ICollection<T>
        /// interface. The other Add method returns bool indicating whether item was added.
        /// </summary>
        /// <param name="item">item to add</param>
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Remove all items from this set. This clears the elements but not the underlying 
        /// buckets and slots array. Follow this call by TrimExcess to release these.
        /// </summary>
        public void Clear()
        {
            if (_lastIndex > 0)
            {
                // clear the elements so that the gc can reclaim the references.
                // clear only up to _lastIndex for _slots 
                Array.Clear(_slots, 0, _lastIndex);
                Array.Clear(_buckets, 0, _buckets.Length);
                Array.Clear(_orderedList, 0, _count);
                _lastIndex = 0;
                _count = 0;
                _freeList = -1;
            }
            _version++;

            AssertInvariants();
        }

        /// <summary>
        /// Checks if this hashset contains the item
        /// </summary>
        /// <param name="item">item to check for containment</param>
        /// <returns>true if item contained; false if not</returns>
        public bool Contains(T item)
        {
            return FindSlotIndex(item) >= 0;
        }

        /// <summary>
        /// Copy items in this hashset to array, starting at arrayIndex
        /// </summary>
        /// <param name="array">array to add items to</param>
        /// <param name="arrayIndex">index to start at</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, _count);
        }

        /// <summary>
        /// Remove item from this hashset
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
        public bool Remove(T item)
        {
            int orderedIndex = IndexOf(item);
            if (orderedIndex >= 0)
            {
                RemoveAt(orderedIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Number of elements in this hashset
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Whether this is readonly
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable methods

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region List methods

        public int Capacity
        {
            get { return _buckets.Length; }
        }

        private int GetBucketIndex(uint hashCode)
        {
            return (int)(hashCode % (uint)_buckets.Length);
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException("index", index, "OrderedHashSet index is not in the valid range.");
                }
                
                return _slots[_orderedList[index]].value;
            }

            set
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException("index", index, "OrderedHashSet index is not in the range of existing indices.");
                }
                
                int existingOrderedIndex = IndexOf(value);
                if (existingOrderedIndex >= 0 && existingOrderedIndex != index)
                {
                    throw new ArgumentException("The specified value already is in the set at a different index.", "value");
                }

                // TODO: optimize me
                RemoveAt(index);
                Insert(index, value);
            }
        }

        public int IndexOf(T item)
        {
            int slotIndex = FindSlotIndex(item);
            if (slotIndex < 0)
            {
                return -1;
            }

            return FindOrderedIndex(slotIndex);
        }

        /// <summary>
        /// Remove item from this hashset
        /// </summary>
        /// <param name="index">index of the item to remove</param>
        /// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
            {
                throw new ArgumentOutOfRangeException("index", index, "OrderedHashSet index is not in the valid range.");
            }

            int slotIndex = _orderedList[index];
            int bucket = GetBucketIndex(_slots[slotIndex].hashCode);

            // Find previous node in bucket sll
            int last = -1;
            for (int i = _buckets[bucket] - 1; i != slotIndex; i = _slots[i].next)
            {
                Debug.Assert(i >= 0, "OrderedHashSet slot is listed in wrong bucket.");
                if (i < 0)
                {
                    // Don't spin forever, even if this hash set is completely hosed (threading issues might cause this)
                    break;
                }

                last = i;
            }

            // Move this entry to the end, so it's easy to remove from the list
            if (index != _count - 1)
            {
                MoveWithoutRenumbering(index, _count - 1);
            }

            if (last < 0)
            {
                // first in sll; update buckets
                _buckets[bucket] = _slots[slotIndex].next + 1;
            }
            else
            {
                // subsequent entries; update 'next' pointers
                _slots[last].next = _slots[slotIndex].next;
            }
            _slots[slotIndex].hashCode = 0;
            _slots[slotIndex].value = default(T);
            _slots[slotIndex].next = -1;
            _slots[slotIndex].insertionNumber = 0;
            _count--;
            _version++;

            if (_count == 0)
            {
                _lastIndex = 0;
                _freeList = -1;
            }
            else if (slotIndex == _lastIndex - 1)
            {
                _lastIndex -= 1;
                _orderedList[_lastIndex] = 0;
            }
            else
            {
                _slots[slotIndex].next = _freeList;
                _freeList = slotIndex;
            }

            AssertInvariants();
        }

        public void RemoveRange(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > _count)
            {
                throw new ArgumentOutOfRangeException("startIndex", startIndex, "OrderedHashSet startIndex is not in the valid range.");
            }
            if (length < 0 || startIndex + length > _count)
            {
                throw new ArgumentOutOfRangeException("length", length, "OrderedHashSet length is not in the valid range.");
            }
            
            MoveRange(startIndex, _count - length, length);

            int initialCount = _count;
            for (int i = 1; i <= length; ++i)
            {
                RemoveAt(initialCount - i);
            }
        }

        /// <summary>
        /// Inserts the item, or moves it to the specified index.
        /// </summary>
        public void Insert(int index, T item)
        {
            if (index < 0 || index > _count)
            {
                throw new ArgumentOutOfRangeException("index", index, "OrderedHashSet index is not in the valid range.");
            }
            
            int existingSlotIndex = FindSlotIndex(item);
            int existingOrderedIndex;

            // Not present; add new item
            if (existingSlotIndex < 0)
            {
                AddAlways(item);
                existingOrderedIndex = _count - 1;
            }
            // Present; move to new index
            else
            {
                if (index == _count)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Cannot insert at end of ordered set because item is already present.");
                }

                // Update value in case slightly different (like if casing different, even if the comparer is case insensitive).
                _slots[existingSlotIndex].value = item;
                existingOrderedIndex = FindOrderedIndex(existingSlotIndex);
            }

            Move(existingOrderedIndex, index);

            AssertInvariants();
        }

        /// <summary>
        /// Removes an item from one index and inserts it into another, shifting the others over.
        /// </summary>
        public void Move(int sourceIndex, int destIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _count)
            {
                throw new ArgumentOutOfRangeException("sourceIndex", sourceIndex, "OrderedHashSet source index is not in the valid range.");
            }
            if (destIndex < 0 || destIndex >= _count)
            {
                throw new ArgumentOutOfRangeException("destIndex", destIndex, "OrderedHashSet destination index is not in the valid range.");
            }
            Debug.Assert(_count > 0);

            // Move to same index; no change.
            if (sourceIndex == destIndex)
            {
                return;
            }

            MoveWithoutRenumbering(sourceIndex, destIndex);
            Renumber(destIndex, 1);
            _version++;

            AssertInvariants();
        }

        private void MoveWithoutRenumbering(int sourceIndex, int destIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _count)
            {
                throw new ArgumentOutOfRangeException("sourceIndex", sourceIndex, "OrderedHashSet source index is not in the valid range.");
            }
            if (destIndex < 0 || destIndex >= _count)
            {
                throw new ArgumentOutOfRangeException("destIndex", destIndex, "OrderedHashSet destination index is not in the valid range.");
            }
            Debug.Assert(_count > 0);

            // Move to same index; no change.
            if (sourceIndex == destIndex)
            {
                return;
            }

            int movingSlotIndex = _orderedList[sourceIndex];

            // Move intermediate values (inclusive, so no -1 on length)
            if (sourceIndex < destIndex)
            {
                Array.Copy(_orderedList, sourceIndex + 1, _orderedList, sourceIndex, destIndex - sourceIndex);
            }
            else
            {
                Array.Copy(_orderedList, destIndex, _orderedList, destIndex + 1, sourceIndex - destIndex);
            }

            // Set the inserted value
            _orderedList[destIndex] = movingSlotIndex;
        }

        /// <summary>
        /// Moves a range of values from one index to another.
        /// </summary>
        public void MoveRange(int sourceIndex, int destIndex, int length)
        {
            if (sourceIndex < 0 || sourceIndex > _count)
            {
                throw new ArgumentOutOfRangeException("sourceIndex", sourceIndex, "OrderedHashSet source index is not in the valid range.");
            }
            if (destIndex < 0 || destIndex > _count)
            {
                throw new ArgumentOutOfRangeException("destIndex", destIndex, "OrderedHashSet destination index is not in the valid range.");
            }
            if (length < 0 || sourceIndex + length > _count || destIndex + length > _count)
            {
                throw new ArgumentOutOfRangeException("length", length, "OrderedHashSet length is not in the valid range.");
            }
            if (destIndex > sourceIndex && sourceIndex + length > destIndex)
            {
                throw new ArgumentOutOfRangeException("destIndex", length, "OrderedHashSet destIndex must not be within the moved range.");
            }
            
            // Move to same index; no change.
            if (sourceIndex == destIndex || length == 0)
            {
                return;
            }

            int low;
            int high;
            int split;
            if (sourceIndex < destIndex)
            {
                low = sourceIndex;
                high = destIndex + length;
                split = destIndex;
            }
            else
            {
                low = destIndex;
                high = sourceIndex + length;
                split = destIndex + length;
            }

            // In-place partial array shift
            Array.Reverse(_orderedList, low, high - low);
            Array.Reverse(_orderedList, low, split - low);
            Array.Reverse(_orderedList, split, high - split);
            Renumber(destIndex, length);
            _version++;

            AssertInvariants();
        }

        /// <summary>
        /// Swaps the values at two different indexes.
        /// </summary>
        public void Swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _count)
            {
                throw new ArgumentOutOfRangeException("indexA", indexA, "OrderedHashSet index is not in the valid range.");
            }
            if (indexB < 0 || indexB >= _count)
            {
                throw new ArgumentOutOfRangeException("indexB", indexB, "OrderedHashSet index is not in the valid range.");
            }
            Debug.Assert(_count > 0);

            // Can be same index
            int slotA = _orderedList[indexA];
            int slotB = _orderedList[indexB];

            int tempInsertionNumber = _slots[slotA].insertionNumber;
            _slots[slotA].insertionNumber = _slots[slotB].insertionNumber;
            _slots[slotB].insertionNumber = tempInsertionNumber;

            _orderedList[indexA] = slotB;
            _orderedList[indexB] = slotA;
        }

        /// <summary>
        /// Stable sort.
        /// Currently allocates, though this could be fixed.
        /// </summary>
        public void StableSort(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            
            Comparison<int> slotComparison = (left, right) =>
            {
                int compare = comparer.Compare(_slots[left].value, _slots[right].value);
                if (compare == 0) compare = _slots[left].insertionNumber.CompareTo(_slots[right].insertionNumber);
                return compare;
            };

            FunctionalComparer<int> indexComparer = new FunctionalComparer<int>(slotComparison);
            SortInternal(indexComparer);
        }

        /// <summary>
        /// Stable sort.
        /// Currently allocates, though this could be fixed.
        /// </summary>
        public void StableSort(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException("comparison");
            
            Comparison<int> slotComparison = (left, right) =>
            {
                int compare = comparison(_slots[left].value, _slots[right].value);
                if (compare == 0) compare = _slots[left].insertionNumber.CompareTo(_slots[right].insertionNumber);
                return compare;
            };

            FunctionalComparer<int> indexComparer = new FunctionalComparer<int>(slotComparison);
            SortInternal(indexComparer);
        }

        private void SortInternal(FunctionalComparer<int> slotIndexComparer)
        {
            Array.Sort(_orderedList, 0, _count, slotIndexComparer);
            Rehash();
            AssertInvariants();
        }

        #endregion

        #region ISet methods

        /// <summary>
        /// Add item to this HashSet. Returns bool indicating whether item was added (won't be 
        /// added if already present)
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if added, false if already present</returns>
        public bool Add(T item)
        {
            if (FindSlotIndex(item) >= 0)
            {
                return false;
            }

            AddAlways(item);
            return true;
        }

        /// <summary>
        /// Take the union of this HashSet with other. Modifies this set.
        /// </summary>
        /// <remarks>
        /// Implementation note: GetSuggestedCapacity (to increase capacity in advance avoiding 
        /// multiple resizes ended up not being useful in practice; quickly gets to the 
        /// point where it's a wasteful check.
        /// </remarks>
        /// <param name="other">enumerable with items to add</param>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                if (otherAsCollection.Count == 0) {
                    return;
                }

                // There may be overlap and duplicates; this is not entirely conservative
                EnsureCapacity(otherAsCollection.Count);

                // Lists support allocation-free iteration
                IList<T> otherAsList = other as IList<T>;
                if (otherAsList != null) {

                    // If this is empty, we can more efficiently assigned from
                    // another set if it has the same equality comparer
                    if (this.Count == 0) {
                        OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
                        if (otherAsSet != null && this.Count == 0 && AreEqualityComparersEqual(this, otherAsSet)) {
                            RehashFrom(otherAsSet, Capacity);
                            return;
                        }
                    }

                    for (int i = 0; i < otherAsList.Count; ++i) {
                        Add(otherAsList[i]);
                    }
                    return;
                }
            }

            foreach (T item in other)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Takes the intersection of this set with other. Modifies this set.
        /// </summary>
        /// <remarks>
        /// Implementation Notes: 
        /// We get better perf if other is a hashset using same equality comparer, because we 
        /// get constant contains check in other. Resulting cost is O(n1) to iterate over this.
        /// 
        /// If we can't go above route, iterate over the other and mark intersection by checking
        /// contains in this. Then loop over and delete any unmarked elements. Total cost is n2+n1. 
        /// 
        /// Attempts to return early based on counts alone, using the property that the 
        /// intersection of anything with the empty set is the empty set.
        /// </remarks>
        /// <param name="other">enumerable with items to add </param>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // intersection of anything with empty set is empty set, so return if count is 0
            if (_count == 0)
            {
                return;
            }

            // set intersecting with itself is the same set
            if (other == this)
            {
                return;
            }

            // if other is empty, intersection is empty set; remove all elements and we're done
            // can only figure this out if implements ICollection<T>. (IEnumerable<T> has no count)
            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                if (otherAsCollection.Count == 0)
                {
                    Clear();
                    return;
                }

                OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
                // faster if other is a hashset using same equality comparer; so check 
                // that other is a hashset using the same equality comparer.
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
                {
                    for (int i = _count - 1; i >= 0; --i)
                    {
                        T item = _slots[_orderedList[i]].value;
                        if (!otherAsSet.Contains(item))
                        {
                            RemoveAt(i);
                        }
                    }
                    return;
                }
            }

            IntersectWithEnumerable(other);
        }

        /// <summary>
        /// Remove items in other from this set. Modifies this set.
        /// </summary>
        /// <param name="other">enumerable with items to remove</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // this is already the empty set; return
            if (_count == 0)
            {
                return;
            }

            // special case if other is this; a set minus itself is the empty set
            if (other == this)
            {
                Clear();
                return;
            }

            IList<T> otherAsList = other as IList<T>;
            if (otherAsList != null)
            {
                for (int i = 0; i < otherAsList.Count; ++i)
                {
                    Remove(otherAsList[i]);
                }
            }
            else
            {
                // remove every element in other from this
                foreach (T element in other)
                {
                    Remove(element);
                }
            }
        }

        /// <summary>
        /// Takes symmetric difference (XOR) with other and this set. Modifies this set.
        /// </summary>
        /// <param name="other">enumerable with items to XOR</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // if set is empty, then symmetric difference is other
            if (_count == 0)
            {
                UnionWith(other);
                return;
            }

            // special case this; the symmetric difference of a set with itself is the empty set
            if (other == this)
            {
                Clear();
                return;
            }

            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null)
            {
                if (otherAsCollection.Count == 0)
                {
                    // Nothing to do.
                    return;
                }

                OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
                // If other is a HashSet, it has unique elements according to its equality comparer,
                // but if they're using different equality comparers, then assumption of uniqueness
                // will fail. So first check if other is a hashset using the same equality comparer;
                // symmetric except is a lot faster and avoids bit array allocations if we can assume
                // uniqueness
                if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
                {
                    int otherCount = otherAsSet.Count;
                    for (int i = 0; i < otherCount; ++i)
                    {
                        T item = otherAsSet[i];
                        if (!Remove(item))
                        {
                            AddAlways(item);
                        }
                    }
                    return;
                }
            }

            SymmetricExceptWithEnumerable(other);
        }

        /// <summary>
        /// Checks if this is a subset of other.
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If this is the empty set, then it's a subset of anything, including the empty set
        /// 2. If other has unique elements according to this equality comparer, and this has more
        /// elements than other, then it can't be a subset.
        /// 
        /// Furthermore, if other is a hashset using the same equality comparer, we can use a 
        /// faster element-wise check.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a subset of other; false if not</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // The empty set is a subset of any set
            if (_count == 0)
            {
                return true;
            }

            // Set is always a subset of itself
            if (other == this)
            {
                return true;
            }

            OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
            // faster if other has unique elements according to this equality comparer; so check 
            // that other is a hashset using the same equality comparer.
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                // if this has more elements then it can't be a subset
                if (_count > otherAsSet.Count)
                {
                    return false;
                }

                // already checked that we're using same equality comparer. simply check that 
                // each element in this is contained in other.
                return otherAsSet.ContainsAllElements(this);
            }
            else
            {
                ElementCount result = CheckUniqueAndUnfoundElements(other, false);
                return (result.uniqueCount == _count && result.unfoundCount >= 0);
            }
        }

        /// <summary>
        /// Checks if this is a proper subset of other (i.e. strictly contained in)
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If this is the empty set, then it's a proper subset of a set that contains at least
        /// one element, but it's not a proper subset of the empty set.
        /// 2. If other has unique elements according to this equality comparer, and this has >=
        /// the number of elements in other, then this can't be a proper subset.
        /// 
        /// Furthermore, if other is a hashset using the same equality comparer, we can use a 
        /// faster element-wise check.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a proper subset of other; false if not</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // no set is a proper subset of itself.
            if (other == this)
            {
                return false;
            }

            OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
            // faster if other is a hashset (and we're using same equality comparer)
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                if (_count >= otherAsSet.Count)
                {
                    return false;
                }
                // this has strictly less than number of items in other, so the following
                // check suffices for proper subset.
                return otherAsSet.ContainsAllElements(this);
            }

            ElementCount result = CheckUniqueAndUnfoundElements(other, false);
            return (result.uniqueCount == _count && result.unfoundCount > 0);
        }

        /// <summary>
        /// Checks if this is a superset of other
        /// 
        /// Implementation Notes:
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If other has no elements (it's the empty set), then this is a superset, even if this
        /// is also the empty set.
        /// 2. If other has unique elements according to this equality comparer, and this has less 
        /// than the number of elements in other, then this can't be a superset
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a superset of other; false if not</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // a set is always a superset of itself
            if (other == this)
            {
                return true;
            }

            // try to fall out early based on counts
            OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
            // try to compare based on counts alone if other is a hashset with
            // same equality comparer
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                if (otherAsSet.Count > _count)
                {
                    return false;
                }
            }

            return ContainsAllElements(other);
        }

        /// <summary>
        /// Checks if this is a proper superset of other (i.e. other strictly contained in this)
        /// 
        /// Implementation Notes: 
        /// This is slightly more complicated than above because we have to keep track if there
        /// was at least one element not contained in other.
        /// 
        /// The following properties are used up-front to avoid element-wise checks:
        /// 1. If this is the empty set, then it can't be a proper superset of any set, even if 
        /// other is the empty set.
        /// 2. If other is an empty set and this contains at least 1 element, then this is a proper
        /// superset.
        /// 3. If other has unique elements according to this equality comparer, and other's count
        /// is greater than or equal to this count, then this can't be a proper superset
        /// 
        /// Furthermore, if other has unique elements according to this equality comparer, we can
        /// use a faster element-wise check.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if this is a proper superset of other; false if not</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // the empty set isn't a proper superset of any set.
            if (_count == 0)
            {
                return false;
            }

            // a set is never a strict superset of itself
            if (other == this)
            {
                return false;
            }

            OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
            // faster if other is a hashset with the same equality comparer
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                if (otherAsSet.Count >= _count)
                {
                    return false;
                }
                // now perform element check
                return ContainsAllElements(otherAsSet);
            }

            // couldn't fall out in the above cases; do it the long way
            ElementCount result = CheckUniqueAndUnfoundElements(other, true);
            return (result.uniqueCount < _count && result.unfoundCount == 0);
        }

        /// <summary>
        /// Checks if this set overlaps other (i.e. they share at least one item)
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true if these have at least one common element; false if disjoint</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (_count == 0)
            {
                return false;
            }

            // set overlaps itself
            if (other == this)
            {
                return true;
            }

            IList<T> otherAsList = other as IList<T>;
            if (otherAsList != null)
            {
                for (int i = 0; i < otherAsList.Count; ++i)
                {
                    T element = otherAsList[i];
                    if (Contains(element))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                foreach (T element in other)
                {
                    if (Contains(element))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Checks if this and other contain the same elements. This is set equality: 
        /// duplicates and order are ignored
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            // a set is equal to itself
            if (other == this)
            {
                return true;
            }

            OrderedHashSet<T> otherAsSet = other as OrderedHashSet<T>;
            // faster if other is a hashset and we're using same equality comparer
            if (otherAsSet != null && AreEqualityComparersEqual(this, otherAsSet))
            {
                // attempt to return early: since both contain unique elements, if they have 
                // different counts, then they can't be equal
                if (_count != otherAsSet.Count)
                {
                    return false;
                }

                // already confirmed that the sets have the same number of distinct elements, so if
                // one is a superset of the other then they must be equal
                return ContainsAllElements(otherAsSet);
            }
            else
            {
                ICollection<T> otherAsCollection = other as ICollection<T>;
                if (otherAsCollection != null)
                {
                    // If other count is smaller, they can't be equal
                    if (_count > otherAsCollection.Count)
                    {
                        return false;
                    }
                }
                ElementCount result = CheckUniqueAndUnfoundElements(other, true);
                return (result.uniqueCount == _count && result.unfoundCount == 0);
            }
        }

        public void CopyTo(T[] array) { CopyTo(array, 0, _count); }

        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null) throw new ArgumentNullException("array");
            // check array index valid index into array
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "Non-negative number required.");
            // also throw if count less than 0
            if (count < 0) throw new ArgumentOutOfRangeException("count", count, "Non-negative number required.");

            // will array, starting at arrayIndex, be able to hold elements? Note: not
            // checking arrayIndex >= array.Length (consistency with list of allowing
            // count of 0; subsequent check takes care of the rest)
            if (arrayIndex > array.Length || count > array.Length - arrayIndex)
            {
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
            }

            for (int i = 0; i < count; ++i)
            {
                array[arrayIndex + i] = _slots[_orderedList[i]].value;
            }
        }

        /// <summary>
        /// Remove elements that match specified predicate. Returns the number of elements removed
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException("match");

            int numRemoved = 0;
            for (int i = _count - 1; i >= 0; --i)
            {
                // cache value in case delegate removes it
                T value = _slots[i].value;
                if (match(value))
                {
                    // check again that remove actually removed it
                    RemoveAt(i);
                    numRemoved++;
                }
            }
            return numRemoved;
        }

        /// <summary>
        /// Gets the IEqualityComparer that is used to determine equality of keys for 
        /// the HashSet.
        /// </summary>
        public IEqualityComparer<T> Comparer
        {
            get
            {
                return _comparer;
            }
        }

        /// <summary>
        /// Sets the capacity of this list to the size of the list (rounded up to nearest prime),
        /// unless count is 0, in which case we release references.
        /// 
        /// This method can be used to minimize a list's memory overhead once it is known that no
        /// new elements will be added to the list. To completely clear a list and release all 
        /// memory referenced by the list, execute the following statements:
        /// 
        /// list.Clear();
        /// list.TrimExcess(); 
        /// </summary>
        public void TrimExcess()
        {
            Debug.Assert(_count >= 0, "_count is negative");

            int newSize = HashHelpers.GetPrime(_count);
            if (newSize >= Capacity)
            {
                return;
            }

            RehashFrom(this, newSize);
        }

        /// <summary>
        /// Ensures that the HashSet has at least the given capacity.
        /// Does not decrease the size.
        /// </summary>
        public void EnsureCapacity(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("capacity", capacity, "Reserved capacity is negative.");

            if (capacity <= Capacity)
            {
                return;
            }

            capacity = Math.Max(HashHelpers.GetPrime(capacity), HashHelpers.ExpandPrime(_count + 1));

            RehashFrom(this, capacity);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Expand to new capacity. New capacity is next prime greater than or equal to suggested 
        /// size. This is called when the underlying array is filled. This performs no 
        /// defragmentation, allowing faster execution; note that this is reasonable since 
        /// AddIfNotPresent attempts to insert new elements in re-opened spots.
        /// </summary>
        /// <param name="sizeSuggestion"></param>
        private void IncreaseCapacity()
        {
            int newSize = HashHelpers.ExpandPrime(_count + 1);
            if (newSize <= _count)
            {
                throw new ArgumentException("OrderedHashSet capacity is too big.");
            }

            // Able to increase capacity; copy elements to larger array and rehash
            RehashFrom(this, newSize);
        }

        /// <summary>
        /// Rehashes this set, organizing it more efficiently.
        /// </summary>
        public void Rehash()
        {
            RehashFrom(this, Capacity);
        }

        /// <remarks>
        /// Set the underlying buckets array to size newSize and rehash.  Note that newSize
        /// *must* be a prime.  It is very likely that you want to call IncreaseCapacity()
        /// instead of this method.
        /// </remarks>
        private void RehashFrom(OrderedHashSet<T> source, int newCapacity)
        {
            Debug.Assert(newCapacity >= 0);
            Debug.Assert(newCapacity >= source._count);
            Debug.Assert(newCapacity == 0 || HashHelpers.GetPrime(newCapacity) == newCapacity);

            int count = source._count;

            //using (ProfileUtil.PushSample("OrderedHashSet.RehashFrom"))
            {
                if (newCapacity == 0)
                {
                    // if count is zero, clear references
                    _buckets = ArrayUtil.Empty<int>();
                    _slots = ArrayUtil.Empty<Slot>();
                    _orderedList = ArrayUtil.Empty<int>();
                }
                else
                {
                    bool reuseSlots = (newCapacity == this.Capacity);

                    Slot[] newSlots;
                    int[] newBuckets;
                    int[] newOrderedList;
                    if (reuseSlots)
                    {
                        newBuckets = _buckets;
                        newSlots = _slots;
                        newOrderedList = _orderedList;
                    }
                    else
                    {
                        newBuckets = new int[newCapacity];
                        newSlots = new Slot[newCapacity];
                        newOrderedList = new int[newCapacity];
                    }

                    // Copy value and hash code from source to beginning of newSlots
                    // Swap if doing an in-place rehash
                    if (reuseSlots && ReferenceEquals(source, this))
                    {
                        for (int i = 0; i < count; ++i)
                        {
                            int slotIndex = _orderedList[i];
                            // If already processed, follow indirections
                            // Loop should always converge because we always point higher
                            while (slotIndex < i)
                            {
                                Debug.Assert(_orderedList[slotIndex] > slotIndex);
                                slotIndex = _orderedList[slotIndex];
                            }

                            // If needed, swap the value
                            if (slotIndex > i)
                            {
                                T value = _slots[slotIndex].value;
                                uint hashCode = _slots[slotIndex].hashCode;
                                _slots[slotIndex].value = _slots[i].value;
                                _slots[slotIndex].hashCode = _slots[i].hashCode;
                                _slots[i].value = value;
                                _slots[i].hashCode = hashCode;
                            }
                            else
                            {
                                // Already at correct index
                            }

                            // Note where we swapped to
                            _orderedList[i] = slotIndex;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int slotIndex = source._orderedList[i];
                            newSlots[i].value = source._slots[slotIndex].value;
                            newSlots[i].hashCode = source._slots[slotIndex].hashCode;
                        }
                    }

                    // Wipe rest of slots
                    if (reuseSlots)
                    {
                        Array.Clear(_buckets, 0, _buckets.Length);
                        Array.Clear(_slots, count, _slots.Length - count);
                        Array.Clear(_orderedList, 0, _lastIndex);
                    }
                    else
                    {
                        _buckets = newBuckets;
                        _slots = newSlots;
                        _orderedList = newOrderedList;
                    }

                    // Rehash
                    int insertionIncrement = GetInsertionNumberIncrement(Capacity);
                    int insertionNumber = MinInsertionNumber;
                    for (int i = 0; i < count; i++)
                    {
                        int bucket = GetBucketIndex(_slots[i].hashCode);

                        int next = _buckets[bucket] - 1;

                        _buckets[bucket] = i + 1;
                        _orderedList[i] = i;
                        _slots[i].next = next;
                        _slots[i].insertionNumber = insertionNumber;
                        insertionNumber += insertionIncrement;
                    }
                }
                _lastIndex = count;
                _freeList = -1;
                _count = count;

                AssertInvariants();
            }
        }

        /// <summary>
        /// Adds value to HashSet if not contained already
        /// Assumes not present
        /// </summary>
        private void AddAlways(T value)
        {
            int capacity = _buckets.Length;
            if (capacity == 0 || _freeList < 0 && _lastIndex == capacity)
            {
                IncreaseCapacity();
            }

            int previousCount = _count;
            uint hashCode = InternalGetHashCode(value);
            int bucket = GetBucketIndex(hashCode);

            int slotIndex;
            if (_freeList >= 0)
            {
                slotIndex = _freeList;
                _freeList = _slots[slotIndex].next;
            }
            else
            {
                slotIndex = _lastIndex;
                _lastIndex++;
            }

            _slots[slotIndex].hashCode = hashCode;
            _slots[slotIndex].value = value;
            _slots[slotIndex].next = _buckets[bucket] - 1;
            _slots[slotIndex].insertionNumber = MinInsertionNumber;
            _buckets[bucket] = slotIndex + 1;
            _orderedList[previousCount] = slotIndex;
            _count++;

            // Small chance we have to rehash.
            if (_count > 1)
            {
                Renumber(_count - 1, 1);
            }

            _version++;

            AssertInvariants();
        }

        /// <summary>
        /// Checks if this contains of other's elements. Iterates over other's elements and 
        /// returns false as soon as it finds an element in other that's not in this.
        /// Used by SupersetOf, ProperSupersetOf, and SetEquals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool ContainsAllElements(IEnumerable<T> other)
        {
            IList<T> otherAsList = other as IList<T>;
            if (otherAsList != null)
            {
                for (int i = 0; i < otherAsList.Count; ++i)
                {
                    T element = otherAsList[i];
                    if (!Contains(element))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                ICollection<T> otherAsCollection = other as ICollection<T>;
                if (otherAsCollection != null && otherAsCollection.Count == 0)
                {
                    return true;
                }

                foreach (T element in other)
                {
                    if (!Contains(element))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Iterate over other. If contained in this, mark an element in bit array corresponding to
        /// its position in _slots. If anything is unmarked (in bit array), remove it.
        /// 
        /// This attempts to allocate on the stack, if below StackAllocThreshold.
        /// </summary>
        /// <param name="other"></param>
#if ALLOW_UNSAFE
        unsafe
#endif
        private void IntersectWithEnumerable(IEnumerable<T> other)
        {
            // keep track of current last index; don't want to move past the end of our bit array
            // (could happen if another thread is modifying the collection)
            int originalCount = _count;
            int originalLastIndex = _lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            BitHelper existingBitArray;
            if (intArrayLength <= BitHelper.StackAllocThreshold)
            {
#if ALLOW_UNSAFE
                int* bitArrayPtr = stackalloc int[intArrayLength];
#else
                int[] bitArrayPtr = BitHelper.ThreadLocalArray.Value;
#endif
                existingBitArray = new BitHelper(bitArrayPtr, intArrayLength);
            }
            else
            {
                int[] bitArray = new int[intArrayLength];
                existingBitArray = new BitHelper(bitArray, intArrayLength);
            }

            // mark if contains: find index of in slots array and mark corresponding element in bit array
            IList<T> list = other as IList<T>;
            if (list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    int slotIndex = FindSlotIndex(list[i]);
                    if (slotIndex >= 0)
                    {
                        existingBitArray.MarkBit(slotIndex);
                    }
                }
            }
            else
            {
                foreach (T item in other)
                {
                    int slotIndex = FindSlotIndex(item);
                    if (slotIndex >= 0)
                    {
                        existingBitArray.MarkBit(slotIndex);
                    }
                }
            }

            // if anything unmarked, remove it. Perf can be optimized here if BitHelper had a 
            // FindFirstUnmarked method.
            for (int i = originalCount - 1; i >= 0; i--)
            {
                int slotIndex = _orderedList[i];

                if (!existingBitArray.IsMarked(slotIndex))
                {
                    RemoveAt(i);
                }
            }
        }

        /// <remarks>
        /// Implementation notes:
        /// 
        /// Used for symmetric except when other isn't a HashSet. This is more tedious because 
        /// other may contain duplicates. HashSet technique could fail in these situations:
        /// 1. Other has a duplicate that's not in this: HashSet technique would add then 
        /// remove it.
        /// 2. Other has a duplicate that's in this: HashSet technique would remove then add it
        /// back.
        /// In general, its presence would be toggled each time it appears in other. 
        /// 
        /// This technique uses bit marking to indicate whether to add/remove the item. If already
        /// present in collection, it will get marked for deletion. If added from other, it will
        /// get marked as something not to remove.
        ///
        /// </remarks>
        /// <param name="other"></param>
#if ALLOW_UNSAFE
        unsafe
#endif
        private void SymmetricExceptWithEnumerable(IEnumerable<T> other)
        {
            int originalCount = _count;
            int originalLastIndex = _lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            BitHelper existingBitArray;
            if (intArrayLength <= BitHelper.StackAllocThreshold)
            {
#if ALLOW_UNSAFE
                int* buffer = stackalloc int[intArrayLength];
#else
                int[] buffer = BitHelper.ThreadLocalArray.Value;
#endif
                existingBitArray = new BitHelper(buffer, intArrayLength);
            }
            else
            {
                int[] buffer = new int[intArrayLength];

                existingBitArray = new BitHelper(buffer, intArrayLength);
            }

            IList<T> otherAsList = other as IList<T>;
            if (otherAsList != null)
            {
                for (int i = 0; i < otherAsList.Count; ++i)
                {
                    T item = otherAsList[i];

                    int slotIndex = FindSlotIndex(item);
                    if (slotIndex >= 0)
                    {
                        // Mark stuff we couldn't add; we ignore new items later
                        existingBitArray.MarkBit(slotIndex);
                    }
                    else
                    {
                        AddAlways(item);
                    }
                }
            }
            else
            {
                foreach (T item in other)
                {
                    int slotIndex = FindSlotIndex(item);
                    if (slotIndex >= 0)
                    {
                        // Mark stuff we couldn't add; we ignore new items later
                        existingBitArray.MarkBit(slotIndex);
                    }
                    else
                    {
                        AddAlways(item);
                    }
                }
            }

            // If anything marked, remove it
            // We only consider the set of items that was already in set before the operation started
            for (int i = originalCount - 1; i >= 0; i--)
            {
                int slotIndex = _orderedList[i];

                if (existingBitArray.IsMarked(slotIndex))
                {
                    RemoveAt(i);
                }
            }
        }

        /// <remarks>
        /// Determines counts that can be used to determine equality, subset, and superset. This
        /// is only used when other is an IEnumerable and not a HashSet. If other is a HashSet
        /// these properties can be checked faster without use of marking because we can assume 
        /// other has no duplicates.
        /// 
        /// The following count checks are performed by callers:
        /// 1. Equals: checks if unfoundCount = 0 and uniqueFoundCount = _count; i.e. everything 
        /// in other is in this and everything in this is in other
        /// 2. Subset: checks if unfoundCount >= 0 and uniqueFoundCount = _count; i.e. other may
        /// have elements not in this and everything in this is in other
        /// 3. Proper subset: checks if unfoundCount > 0 and uniqueFoundCount = _count; i.e
        /// other must have at least one element not in this and everything in this is in other
        /// 4. Proper superset: checks if unfound count = 0 and uniqueFoundCount strictly less
        /// than _count; i.e. everything in other was in this and this had at least one element
        /// not contained in other.
        /// 
        /// An earlier implementation used delegates to perform these checks rather than returning
        /// an ElementCount struct; however this was changed due to the perf overhead of delegates.
        /// </remarks>
        /// <param name="other"></param>
        /// <param name="returnIfUnfound">Allows us to finish faster for equals and proper superset
        /// because unfoundCount must be 0.</param>
        /// <returns></returns>
#if ALLOW_UNSAFE
        unsafe
#endif
        private ElementCount CheckUniqueAndUnfoundElements(IEnumerable<T> other, bool returnIfUnfound)
        {
            ElementCount result;

            // Special case in case other has no element
            ICollection<T> otherAsCollection = other as ICollection<T>;
            if (otherAsCollection != null && otherAsCollection.Count == 0)
            {
                result.uniqueCount = 0;
                result.unfoundCount = 0;
                return result;
            }

            // need special case in case this has no elements. 
            if (_count == 0)
            {
                int numElementsInOther = 0;
                if (otherAsCollection != null)
                {
                    numElementsInOther = otherAsCollection.Count;
                }
                else
                {
                    var enumerator = other.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        numElementsInOther++;
                    }
                }
                result.uniqueCount = 0;
                result.unfoundCount = numElementsInOther;
                return result;
            }

            int originalLastIndex = _lastIndex;
            int intArrayLength = BitHelper.ToIntArrayLength(originalLastIndex);

            BitHelper uniqueBitArray;
            if (intArrayLength <= BitHelper.StackAllocThreshold)
            {
#if ALLOW_UNSAFE
                int* bitArrayPtr = stackalloc int[intArrayLength];
#else
                int[] bitArrayPtr = BitHelper.ThreadLocalArray.Value;
#endif
                uniqueBitArray = new BitHelper(bitArrayPtr, intArrayLength);
            }
            else
            {
                int[] bitArray = new int[intArrayLength];
                uniqueBitArray = new BitHelper(bitArray, intArrayLength);
            }

            // count of items in other not found in this
            int unfoundCount = 0;
            // count of unique items in other found in this
            int uniqueFoundCount = 0;

            IList<T> otherAsList = other as IList<T>;
            if (otherAsList != null)
            {
                for (int i = 0; i < otherAsList.Count; ++i)
                {
                    T item = otherAsList[i];
                    int slotIndex = FindSlotIndex(item);
                    if (slotIndex >= 0)
                    {
                        if (!uniqueBitArray.IsMarked(slotIndex))
                        {
                            // item hasn't been seen yet
                            uniqueBitArray.MarkBit(slotIndex);
                            uniqueFoundCount++;
                        }
                    }
                    else
                    {
                        unfoundCount++;
                        if (returnIfUnfound)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (T item in other)
                {
                    int slotIndex = FindSlotIndex(item);
                    if (slotIndex >= 0)
                    {
                        if (!uniqueBitArray.IsMarked(slotIndex))
                        {
                            // item hasn't been seen yet
                            uniqueBitArray.MarkBit(slotIndex);
                            uniqueFoundCount++;
                        }
                    }
                    else
                    {
                        unfoundCount++;
                        if (returnIfUnfound)
                        {
                            break;
                        }
                    }
                }
            }

            result.uniqueCount = uniqueFoundCount;
            result.unfoundCount = unfoundCount;
            return result;
        }

        /// <summary>
        /// Checks if equality comparers are equal. This is used for algorithms that can
        /// speed up if it knows the other item has unique elements. I.e. if they're using 
        /// different equality comparers, then uniqueness assumption between sets break.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        /// <returns></returns>
        private static bool AreEqualityComparersEqual(OrderedHashSet<T> set1, OrderedHashSet<T> set2)
        {
            return set1.Comparer.Equals(set2.Comparer);
        }

        /// <summary>
        /// Workaround Comparers that throw ArgumentNullException for GetHashCode(null).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>hash code</returns>
        private uint InternalGetHashCode(T item)
        {
            if (!(item is T))
            {
                return 0;
            }
            return (uint)_comparer.GetHashCode(item);
        }

        /// <summary>
        /// Used internally by set operations which have to rely on bit array marking. This is like
        /// Contains but returns index in slots array. 
        /// </summary>
        /// <remarks>
        /// Running time: O(1) depending on the hash algorithm, up to O(n)
        /// </remarks>
        private int FindSlotIndex(T item)
        {
            if (_buckets.Length != 0)
            {
                uint hashCode = InternalGetHashCode(item);
                for (int i = _buckets[GetBucketIndex(hashCode)] - 1; i >= 0; i = _slots[i].next)
                {
                    if (_slots[i].hashCode == hashCode && _comparer.Equals(_slots[i].value, item))
                    {
                        return i;
                    }
                }
            }
            // wasn't found
            return -1;
        }

        /// <summary>
        /// Used internally to find the ordered index.
        /// </summary>
        /// <remarks>
        /// Running time: O(log(n)), though it should be pretty cheap
        /// From mscorlib.
        /// </remarks>
        private int FindOrderedIndex(int slotIndex)
        {
            Debug.Assert(slotIndex >= 0);
            Debug.Assert(slotIndex < _slots.Length);
            Debug.Assert(_count > 0);

            int insertionNumber = _slots[slotIndex].insertionNumber;

            int lo = 0;
            int hi = _count - 1;
            
            // Binary search

            // Try the slot itself first
            int index = slotIndex;
            if (index > hi)
            {
                index = lo + ((hi - lo) >> 1);
            }

            while (lo <= hi)
            {
                int iterSlot = _orderedList[index];
                if (iterSlot == slotIndex)
                {
                    return index;
                }

                if (insertionNumber > _slots[iterSlot].insertionNumber)
                {
                    lo = index + 1;
                }
                else
                {
                    hi = index - 1;
                }
                index = lo + ((hi - lo) >> 1);
            }

            Debug.Assert(false, "Unreachable code.");

            return -1;
        }

        /// <summary>
        /// Renumber the insertion number at the end. Amortized O(1), rarely O(n).
        /// </summary>
        private void Renumber(int startIndex, int length)
        {
            //using (ProfileUtil.PushSample("OrderedHashSet<T>.Renumber"))
            {
                Debug.Assert(length > 0);
                Debug.Assert(length <= _count);
                Debug.Assert(startIndex >= 0);
                Debug.Assert(startIndex <= _count);
                Debug.Assert(startIndex + length <= _count);

                // If full set, rehash everything
                if (length == _count)
                {
                    Rehash();
                    return;
                }

                // The minimum (exclusive) and maximum (exclusive) values allowed.
                long lowInsertionNumber;
                long highInsertionNumber;

                // Range is at the beginning of the set
                if (startIndex == 0)
                {
                    Debug.Assert(length < _count);

                    int nextInsertionNumber = _slots[_orderedList[startIndex + length]].insertionNumber;

                    // Underflow check
                    if (nextInsertionNumber < int.MinValue + length)
                    {
                        Rehash();
                        return;
                    }

                    // Try to use MinInsertionNumber; otherwise pack tightly (avoid int.MinValue)
                    lowInsertionNumber = Math.Min(MinInsertionNumber, nextInsertionNumber - length);
                    highInsertionNumber = nextInsertionNumber;
                }
                // Range is at the middle of the set
                else if (startIndex + length < _count)
                {
                    lowInsertionNumber = _slots[_orderedList[startIndex - 1]].insertionNumber;
                    highInsertionNumber = _slots[_orderedList[startIndex + length]].insertionNumber;
                }
                // Range is the at end of the set
                else
                {
                    Debug.Assert(startIndex > 0);

                    lowInsertionNumber = _slots[_orderedList[startIndex - 1]].insertionNumber;
                    highInsertionNumber = int.MaxValue;
                }

                // If there are enough numbers in range, use them
                long range = (highInsertionNumber - lowInsertionNumber) - 1;
                if (range >= length)
                {
                    int endIndex = startIndex + length;

                    long insertionIncrementLong = Math.Min(range / length, GetInsertionNumberIncrement(Capacity));
                    long insertionNumberLong;
                    // Unless at end, put the remainder in front; inserts tend to be in the same place a lot
                    if (endIndex < _count)
                    {
                        insertionNumberLong = highInsertionNumber - insertionIncrementLong * length;
                    }
                    else
                    {
                        insertionNumberLong = lowInsertionNumber + insertionIncrementLong;
                    }

                    Debug.Assert(insertionIncrementLong >= int.MinValue && insertionIncrementLong <= int.MaxValue);
                    Debug.Assert(insertionNumberLong >= int.MinValue && insertionNumberLong <= int.MaxValue);

                    int insertionIncrement = (int)insertionIncrementLong;
                    int insertionNumber = (int)insertionNumberLong;

                    for (int i = startIndex; i < endIndex; ++i)
                    {
                        _slots[_orderedList[i]].insertionNumber = insertionNumber;
                        insertionNumber += insertionIncrement;
                    }
                }
                // Otherwise, rehash everything (improves locality of reference)
                else
                {
                    Rehash();
                }
            }
        }

        private static int GetInsertionNumberIncrement(int capacity)
        {
            const int MinShiftThreshold = 16;
            const int CountShift = 2;

            Debug.Assert(capacity >= 0);
            Debug.Assert(MaxInsertionNumberShift > (32 - MinShiftThreshold) / CountShift);

            capacity >>= MinShiftThreshold;
            int shift = MaxInsertionNumberShift;

            while (capacity > 0)
            {
                capacity >>= CountShift;
                shift -= 1;
            }

            int increment = 1 << shift;

            Debug.Assert(int.MaxValue >= (ulong)capacity * (ulong)increment);

            return increment;
        }

        #endregion

        #region ICollection Implementation

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        #endregion

        #region IList Implementation

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                this[index] = (T)value;
            }
        }

        int IList.Add(object value)
        {
            return Add((T)value) ? 1 : 0;
        }

        void IList.Clear()
        {
            Clear();
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        public T[] ToArray()
        {
            if (_count == 0)
            {
                return ArrayUtil.Empty<T>();
            }

            T[] array = new T[_count];
            CopyTo(array, 0);
            return array;
        }

        #endregion

        // used for set checking operations (using enumerables) that rely on counting
        private struct ElementCount
        {
            internal int uniqueCount;
            internal int unfoundCount;
        }

        private struct Slot
        {
            internal uint hashCode;
            /// <summary>
            /// Index of next entry in the bucket or free list sll, -1 if last
            /// </summary>
            internal int next;
            /// <summary>
            /// Ordering value for binary search to find ordered index
            /// </summary>
            internal int insertionNumber;

            internal T value;
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private OrderedHashSet<T> _set;
            private int _index;
            private int _version;
            private T _current;

            internal Enumerator(OrderedHashSet<T> set)
            {
                _set = set;
                _index = 0;
                _version = set._version;
                _current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_version != _set._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                if (_index < _set.Count)
                {
                    _current = _set[_index];
                    _index++;
                    return true;
                }
                _current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return _current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _set._lastIndex + 1)
                    {
                        throw new InvalidOperationException("Enumeration has either not started or has already finished.");
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _set._version)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }

                _index = 0;
                _current = default(T);
            }
        }

        // Uncomment to run really expensive checking
        //[Conditional("UNITY_ASSERTIONS")]
        [Conditional("INVALID")]
        private void AssertInvariants()
        {
            Debug.Assert(_buckets != null);
            Debug.Assert(_slots != null);
            Debug.Assert(_orderedList != null);
            Debug.Assert(_slots.Length == _buckets.Length);
            Debug.Assert(_orderedList.Length == _slots.Length);
            Debug.Assert(_lastIndex >= 0);
            Debug.Assert(_lastIndex <= _buckets.Length);
            Debug.Assert(_count >= 0);
            Debug.Assert(_count <= _lastIndex);
            
            if (_buckets.Length < 1000)
            {
                // Traverse sll for all slots in buckets.
                int traverseCount = 0;
                for (int i = 0; i < _buckets.Length; ++i)
                {
                    for (int slotIndex = _buckets[i] - 1; slotIndex >= 0; slotIndex = _slots[slotIndex].next)
                    {
                        // Check slot index
                        Debug.Assert(slotIndex >= 0);
                        Debug.Assert(slotIndex < _lastIndex);
                        traverseCount += 1;
                        Debug.Assert(traverseCount <= _count);

                        // Check hash code
                        Debug.Assert(_slots[slotIndex].hashCode == InternalGetHashCode(_slots[slotIndex].value));
                    }
                }
                Debug.Assert(_count == traverseCount);
            }

            if (_lastIndex - _count < 1000)
            {
                int traverseCount = _count;
                // Traverse freelist in sll
                for (int slotIndex = _freeList; slotIndex != -1; slotIndex = _slots[slotIndex].next)
                {
                    // Check slot index
                    Debug.Assert(slotIndex >= 0);
                    Debug.Assert(slotIndex < _lastIndex);
                    traverseCount += 1;
                    Debug.Assert(traverseCount <= _lastIndex);

                    // Check empty hash code
                    Debug.Assert(0 == _slots[slotIndex].hashCode);

                    // Check empty insertion number.
                    Debug.Assert(0 == _slots[slotIndex].insertionNumber);
                }
                Debug.Assert(_lastIndex == traverseCount);
            }

            if (_count < 1000)
            {
                long minInsertionNumber = int.MinValue;
                for (int i = 0; i < _count; i++)
                {
                    Debug.Assert(minInsertionNumber < int.MaxValue);

                    int orderedIndex = _orderedList[i];
                    Debug.Assert(orderedIndex >= 0);
                    Debug.Assert(orderedIndex < _lastIndex);
                    int insertionNumber = _slots[orderedIndex].insertionNumber;
                    Debug.Assert(insertionNumber < int.MaxValue);
                    Debug.Assert(insertionNumber >= minInsertionNumber);
                    minInsertionNumber = insertionNumber + 1;
                }
            }
        }
    }
}