using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FRG.Core {

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    /// <summary>
    /// Static util class for array/list comparison, copying, etc
    /// </summary>
    public static class ArrayUtil {
        
        /// <summary>
        /// An empty typed array.
        /// </summary>
        [DebuggerStepThrough]
        public static T[] Empty<T>()
        {
            return TypedStatics<T>.EmptyArray;
        }

        private static class TypedStatics<T>
        {
            public static readonly T[] EmptyArray = new T[0];
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return (collection == null || collection.Count == 0);
        }

#if !MAX_COMPATIBILTY
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Deep list comparison, using default equality comparer
        /// </summary>
        /// <param name="sequenceA">The first list to compare</param>
        /// <param name="sequenceB">The second list to compare</param>
        public static bool CompareLists<T>(IEnumerable<T> sequenceA, IEnumerable<T> sequenceB)
        {
            return CompareLists<T>(sequenceA, sequenceB, SafeEqualityComparer<T>.Default);
        }
#endif

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Deep list comparison, using custom equality comparer
        /// </summary>
        /// <param name="sequenceA">The first list to compare</param>
        /// <param name="sequenceB">The second list to compare</param>
        /// <param name="comparer">The comparer to use.</param>
        public static bool CompareLists<T>(IEnumerable<T> sequenceA, IEnumerable<T> sequenceB, IEqualityComparer<T> comparer)
        {
            // check null
            if (ReferenceEquals(sequenceA, null)) return (ReferenceEquals(sequenceB, null));
            if (ReferenceEquals(sequenceB, null)) return false;

            // Try for allocationless compare.
            if (sequenceA is IList<T> && sequenceB is IList<T>)
            {
                IList<T> listA = (IList<T>)sequenceA;
                IList<T> listB = (IList<T>)sequenceB;

                int count = listA.Count;
                if (listB.Count != count) return false;

                //iterate by index and compare each element for each list
                for (int i = 0; i < count; ++i)
                {
                    if (!comparer.Equals(listA[i], listB[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            // Fall back to LINQ.
            else
            {
                return sequenceA.SequenceEqual(sequenceB);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Return whether the collection contains ALL specified values
        /// </summary>
        /// <param name="enumerable">The collection to iterate through</param>
        /// <param name="values">The values to search for</param>
        public static bool ContainsAllValues<T>(this IEnumerable<T> enumerable, params T[] values)
        {
            return ContainsAllValues<T>(enumerable, (IEnumerable<T>)values);
        }

        public static bool ContainsAllValues<T>(this IEnumerable<T> enumerable, IEnumerable<T> values) {
            // NOTE: Following the pattern of Enumerable.Contains().
            ICollection<T> collection = enumerable as ICollection<T> ?? enumerable.ToArray();
            if (collection.Count == 0) return true;

            IList<T> list = values as IList<T> ?? values.ToArray();
            int count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                T value = list[i];
                if (!collection.Contains(value))
                {
                    return false;
                }
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary> Return whether the list contains any of the specified values </summary>
        public static bool ContainsAnyValues<T>(this IEnumerable<T> enumerable, params T[] values)
        {
            return ContainsAnyValues<T>(enumerable, (IEnumerable<T>)values);
        }

        public static bool ContainsAnyValues<T>(this IEnumerable<T> enumerable, IEnumerable<T> values) {
            // NOTE: Following the pattern of Enumerable.Contains().
            ICollection<T> collection = enumerable as ICollection<T> ?? enumerable.ToArray();
            if (collection.Count == 0) return false;

            IList<T> list = values as IList<T> ?? values.ToArray();
            int count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                T value = list[i];
                if (collection.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }

#if !MAX_COMPATIBILTY
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary> Return the first index within a list that contains the specified value </summary>
        public static int FindIndexWithValue<T>(this IEnumerable<T> enumerable, T value)
        {
            // NOTE: Following the pattern of Enumerable.Contains().
            IList<T> list = enumerable as IList<T>;
            if (list != null) {
                return list.IndexOf(value);
            }
            return FindIndexWithValue(enumerable, value, null);
        }

        public static int FindIndexWithValue<T>(this IEnumerable<T> enumerable, T value, IEqualityComparer<T> equalityComparer)
        {
            equalityComparer = equalityComparer ?? SafeEqualityComparer<T>.Default;

            IList<T> list = enumerable as IList<T>;
            if (list != null)
            {
                return list.IndexOf(value);
            }

            var itor = enumerable.GetEnumerator();
            for (int i = 0; itor.MoveNext(); ++i)
            {
                if (equalityComparer.Equals(value, itor.Current)) return i;
            }

            return -1;
        }
#endif

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Return a copy of a list with the specified element copy function
        /// </summary>
        /// <param name="listToCopy">List to copy from</param>
        /// <param name="converter">Element copy function; if null, will shallow-copy the list</param>
        public static List<TOutput> CopyList<T, TOutput>(IList<T> listToCopy, Func<T, TOutput> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (listToCopy == null) return null;
            List<TOutput> ret = new List<TOutput>(listToCopy.Count);
            for (int i = 0; i < listToCopy.Count; ++i)
                ret.Add(converter(listToCopy[i]));
            return ret;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Return a copy of an array with the specified element copy function
        /// </summary>
        /// <param name="listToCopy">List to copy from</param>
        /// <param name="converter">Element copy function; if null, will shallow-copy the array</param>
        public static TOutput[] CopyArray<T, TOutput>(IList<T> listToCopy, Func<T, TOutput> converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException("converter");
            }

            if (listToCopy == null)
                return null;
            TOutput[] ret = new TOutput[listToCopy.Count];
            for (int i = 0; i < listToCopy.Count; ++i)
                ret[i] = converter(listToCopy[i]);
            return ret;
        }

        /// <summary>
        /// Create a uniform 2D array with the specified dimensions and initial field value
        /// </summary>
        /// <remarks>
        /// Prefer multidimensional arrays when possible (for square arrays).
        /// Jagged arrays (T[][]) are faster in .NET, apparently by a factor of 2 because of optimizations,
        /// but mono does not have this problem, and array access is not our bottleneck.
        /// </remarks>
        public static T[,] Create2DArray<T>(int sizeX, int sizeY, T initialValue_ = default(T)) {
            T[,] ret = new T[sizeX,sizeY];
            for(int x=0; x<sizeX; ++x)
                for(int y=0; y<sizeY; ++y)
                    ret[x,y] = initialValue_;
            return ret;
        }

        public enum ArrayAlgorithmType {
            Iterative,       //more memory, heap allocs, but usually faster
            RecursiveSwap,   //less memory, generally slower, swaps elements, but only stack-allocs; only use on small collections
        }

#if !MAX_COMPATIBILTY
        /// <summary>
        /// Randomly traverse a 2D-array
        /// </summary>
        /// <param name="func_">Function to call on each element; algorithm stops when func_ returns false</param>
        /// <param name="modFunc_">Function called on each element; each element will be set to the return-value of this function</param>
        public static void RandomTraverse<T>(T[,] array_, System.Func<T,bool> func_, System.Func<T,T> modFunc_ = null, ArrayAlgorithmType algType_ = ArrayAlgorithmType.Iterative) {
            if(array_ == null) return;
            if(func_ == null) func_ = _DefaultFunc_RandomTraverseIterationFunc<T>;
            _RandomTraverse<T>(array_, func_, modFunc_, algType_);
        }
        private static bool _DefaultFunc_RandomTraverseIterationFunc<T>(T val_) { return true; }

        /// <summary>
        /// Body function for RandomTraverse
        /// </summary>
        /// <param name="func_">Function to call on each randomly selected element; return whether to continue iteration</param>
        /// <param name="modificationFunc_">If this is called from the value-type method, we may opt for a function which modifies the values</param>
        /// <param name="algType_">The type of algorithm to enforce for iteration</param>
        private static void _RandomTraverse<T>(T[,] array_, System.Func<T, bool> func_, System.Func<T,T> modificationFunc_ = null, ArrayAlgorithmType algType_ = ArrayAlgorithmType.Iterative) {
            if(array_ == null) return;

            //correct the function we pass into the body methods depending on if we specified a modification function or not
            System.Func<int,int,bool> correctedFunc;
            if(modificationFunc_ != null) {
                correctedFunc = new Func<int,int,bool>( (x,y) => {
                    array_[x,y] = modificationFunc_(array_[x,y]);
                    return func_(array_[x,y]);
                });
            }else{
                correctedFunc = new Func<int,int,bool>( (x,y) => {
                    return func_(array_[x,y]);
                });
            }

            switch(algType_) {
                //force the stack/recurse method
                case ArrayAlgorithmType.RecursiveSwap:
                    _Random2DArrayTraverse_Recurse<T>(correctedFunc, array_, 0, 0, array_.GetLength(0)-1, array_.GetLength(1)-1);
                    break;
                //force the heap-alloc method
                case ArrayAlgorithmType.Iterative:
                    _Random2DArrayTraverse_Alloc(correctedFunc, array_);
                    break;
            }
        }

        /// <summary>
        /// Recursive random 2D-array traversal, only allocates on the stack, though execution time is generally SLOWER than the heap-alloc method
        /// </summary>
        private static void _Random2DArrayTraverse_Recurse<T>(System.Func<int,int, bool> func_, T[,] array_, int xCurr_, int yCurr_, int xEnd_, int yEnd_, System.Func<T,T> modifyFunc_ = null) {
            //if we've reached the end, don't recurse anymore
            if(xCurr_ == xEnd_ && yCurr_ == yEnd_) {
                func_(xCurr_,yCurr_);
            }else{
                int xRand;
                int yRand;
                {
                    int nextRand;
                    {
                        int currentCount = (xCurr_ + yCurr_ * (xEnd_ + 1)); //the current number of elements we've iterated over
                        int totalCount = (xEnd_ + 1) * (yEnd_ + 1); //total number of elements in the array
                        nextRand = currentCount + UnityEngine.Random.Range(0, totalCount - currentCount);
                    }
                    //we then resolve the 2D indices of nextRand below
                    xRand = nextRand % (xEnd_ + 1);
                    yRand = nextRand / (xEnd_ + 1);
                }

                //do a temporary element swap, and call the function at the current index
                T temp = array_[xCurr_, yCurr_];
                array_[xCurr_,yCurr_] = array_[xRand, yRand];
                array_[xRand, yRand] = temp;

                //call the function on the current index
                if(func_(xCurr_,yCurr_)) {
                    //do the recursive call on the next elements (x+1,y) if x < xEnd_; otherwise, (0,y+1)
                    _Random2DArrayTraverse_Recurse<T>(func_, array_, (xCurr_ >= xEnd_ ? 0 : xCurr_ + 1), (xCurr_ >= xEnd_ ? yCurr_ + 1 : yCurr_), xEnd_, yEnd_, modifyFunc_);
                }

                //un-swap the temporarily swapped elements
                temp = array_[xRand, yRand];
                array_[xRand, yRand] = array_[xCurr_,yCurr_];
                array_[xCurr_, yCurr_] = temp;
            }
        }

        /// <summary>
        /// Iterative random 2D-array traversal; generally faster execution time than the stack-alloc recursive method, but allocates
        /// </summary>
        private static void _Random2DArrayTraverse_Alloc<T>(System.Func<int,int, bool> func_, T[,] array_) {
            int sizeX = array_.GetLength(0);
            int sizeY = array_.GetLength(1);
            int size = sizeX * sizeY;
            int[] swapIndices = new int[size];
            for(int i=0; i<size; ++i) swapIndices[i] = i;
            RandomizeList<int>(swapIndices);

            for(int i=size-1; i>=0; --i) {
                int ind = swapIndices[i];
                if(!func_(ind % (sizeX), ind / (sizeX))) break;
            }
        }

        /// <summary>
        /// Randomly iterate over the given collection, calling func_() on every element
        /// </summary>
        public static void RandomTraverse<T>(IList<T> list_, System.Action<T> func_) {
            _RandomTraverseFunctional<T>(list_, 0, (x) => { func_(x); return true; });
        }

        /// <summary>
        /// Randomly iterate over the given collection, calling func_() on every element
        /// </summary>
        /// <param name="func_">Called on all elements randomly; if this returns false, iteration stops</param>
        public static void RandomTraverse<T>(IList<T> list_, System.Func<T, bool> func_) {
            _RandomTraverseFunctional<T>(list_, 0, func_);
        }

        //method body for RandomTraverse, being passed a bool-method; iterates as long as the function returns true
        private static void _RandomTraverseFunctional<T>(IList<T> list_, int curr_, System.Func<T, bool> func_) {
            if(curr_ == list_.Count-1) {
                func_(list_[curr_]);
                return;
            }else{
                int randIndex = UnityEngine.Random.Range(curr_, list_.Count);
                T temp = list_[curr_];
                list_[curr_] = list_[randIndex];
                list_[randIndex] = temp;
                
                if(func_(list_[curr_]))
                    _RandomTraverseFunctional<T>(list_, curr_ + 1, func_);

                list_[randIndex] = list_[curr_];
                list_[curr_] = temp;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Shuffles a list in random order
        /// </summary>
        public static void RandomizeList<T>(IList<T> list)
        {
            int n = list.Count;
            T temp;
            int ind;
            while (n > 1) {
                --n;
                ind = UnityEngine.Random.Range(0, n + 1);
                temp = list[ind];
                list[ind] = list[n];
                list[n] = temp;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Shuffles a list in pseudo random order. Same seed always produces the same result on same list.
        /// </summary>
        public static void RandomizeListFromSeed<T>(IList<T> list, int seed)
        {
            var random = new Random(seed);
            int n = list.Count;
            T temp;
            int ind;
            while (n > 1)
            {
                --n;
                ind = random.Next(0, n + 1);
                temp = list[ind];
                list[ind] = list[n];
                list[n] = temp;
            }
        }
#endif

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Extends a list to the given size, padding with default
        /// </summary>
        /// <typeparam name="T">The type of the list</typeparam>
        /// <param name="list">The list to extend</param>
        /// <param name="newSize">The new size of the list</param>
        public static void Extend<T>( this List<T> list, int newSize ) {
            List<T> listList = (List<T>)list;
            if( listList != null ) {
                if( newSize > listList.Count ) {
                    listList.Capacity = Math.Max(listList.Capacity, newSize);
                    while( listList.Count < newSize ) {
                        listList.Add( default( T ) );
                    }
                }
                return;
            }
            while( list.Count < newSize ) {
                list.Add( default( T ) );
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Extends a list to the given size, padding with default
        /// </summary>
        /// <typeparam name="T">The type of the list</typeparam>
        /// <param name="list">The list to extend</param>
        /// <param name="newSize">The new size of the list</param>
        public static T[] Extend<T>( T[] list, int newSize ) {
            if( list != null ) {
                if( newSize > list.Length ) {
                    T[] newList = new T[newSize];
                    Array.Copy( list, newList, list.Length );
                    return newList;
                }
            }
            return list;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary> Reset all 2D array fields to the specified value </summary>
        public static void Clear<T>(T[,] array_, T value_) {
            int len0 = array_.GetLength(0);
            int len1 = array_.GetLength(1);
            for(int a=0; a<len0; ++a)
                for(int b=0; b<len1; ++b)
                    array_[a,b] = value_;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary> Reset all 3D array fields to the specified value </summary>
        public static void Clear<T>(T[,,] array_, T value_) {
            int len0 = array_.GetLength(0);
            int len1 = array_.GetLength(1);
            int len2 = array_.GetLength(2);
            for(int a=0; a<len0; ++a)
                for(int b=0; b<len1; ++b)
                    for(int c=0; c<len2; ++c)
                        array_[a,b,c] = value_;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary> Reset all 2D jagged array fields to the specified value </summary>
        public static void Clear<T>(T[][] array_, T value_) {
            for(int i=0; i<array_.Length; ++i)
                for(int j=0; j<array_[i].Length; ++j)
                    array_[i][j] = value_;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary> Reset all 3D jagged array fields to the specified value </summary>
        public static void Clear<T>(T[][][] array_, T value_) {
            for(int i=0; i<array_.Length; ++i)
                for(int j=0; j<array_[i].Length; ++j)
                    for(int k=0; k<array_[i][j].Length; ++k)
                        array_[i][j][k] = value_;
        }

        public static IEnumerable AsEnumerable(this IEnumerator itor_) {
            while(itor_.MoveNext()) yield return itor_.Current;
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> itor_) {
            while(itor_.MoveNext()) yield return itor_.Current;
        }

        /// <summary>
        /// Traverse across 2 coordinates from start to end, calling the specified func on each set of indices
        /// </summary>
        /// <param name="func_">Returns true to continue; returns false to stop</param>
        public static void LinearTraverse_2D(int startX_, int startY_, int endX_, int endY_, System.Func<int,int,bool> func_) {

            if(!func_(startX_,startY_)) return;

            int xDist = (endX_-startX_);
            int yDist = (endY_-startY_);
            int x = startX_;
            int y = startY_;
            int xInc = xDist < 0 ? -1 : 1;
            int yInc = yDist < 0 ? -1 : 1;

            if(x != endX_) {
                x+=xInc;
                while(x != endX_) {
                    if(!func_(x,y)) return;
                    x += xInc;
                }
            }
            while(y != endY_) {
                if(!func_(x,y)) return;
                y += yInc;
            }

            //int xDiff = endX_-startX_;
            //int yDiff = endY_-startY_;
            //int xRatio = yDiff != 0 ? ((endX_-startX_) / (endY_-startY_)) : xDiff;
            //int yRatio = xDiff != 0 ? ((endY_-startY_) / (endX_-startX_)) : yDiff;

            //bool xTrav = (Math.Abs(xRatio) > Math.Abs(yRatio));

            //int x=startX_, y=startY_;
            //int xInc = (endX_>startX_) ? 1 : -1;
            //int yInc = (endY_>startY_) ? 1 : -1;

            //xRatio *= (xRatio < 0 ? -1 : 1);
            //yRatio *= (yRatio < 0 ? -1 : 1);

            //int currStep = 0;
            //while(x != endX_ || y != endY_) {
            //    if(currStep >= (xTrav ? xRatio : yRatio)) {
            //        currStep = 0;
            //        xTrav = !xTrav;
            //    }
            //    if(!func_(x,y)) return;

            //    ++currStep;
            //    if(xTrav) x += xInc;
            //    else y += yInc;
            //}
        }

        /// <summary>
        /// Returns the first set found which has the specified sum.
        /// </summary>
        /// <param name="mapFunction">Function which evaluates the integer-value of an element in the list</param>
        /// <exception cref="InvalidOperationException">Throws exception if no valid combinations exist</exception>
        public static T[] GetValuesWithSum<T>(IEnumerable<T> values, int requiredSum, Func<T, int> mapFunction) {
            if(values == null) throw new ArgumentNullException("values");
            if(mapFunction == null) throw new ArgumentNullException("mapFunction");

            List<T> sorted = new List<T>();
            Stack<T> stack = new Stack<T>();

            foreach(var value in values) {
                sorted.Add(value);
            }

            sorted.Sort((a,b) => mapFunction(b).CompareTo(mapFunction(a)));

            _GetValuesWithSum_Inner(sorted, stack, 0, requiredSum, mapFunction);
            if(stack != null && stack.Count > 0) {
                return stack.ToArray();
            } else {
                throw new InvalidOperationException("There is no combination of <" + typeof(T).Name + "> in the given collection which has a sum of " + requiredSum.ToString());
            }
        }

        private static bool _GetValuesWithSum_Inner<T>(List<T> sorted, Stack<T> stack, int currentIndex, int remainder, Func<T, int> map) {
            while(currentIndex < sorted.Count) {
                T thisElem = sorted[currentIndex];
                int elemValue = map(thisElem);

                //if we've found an element that is equal to the remainder, we are done; push the value and return
                if(elemValue == remainder) {
                    stack.Push(thisElem);
                    return true;
                //if we've found an element with less than the remainder, push this value and continue on
                } else if(elemValue < remainder) {
                    stack.Push(thisElem);

                    if(_GetValuesWithSum_Inner<T>(sorted, stack, currentIndex + 1, remainder - elemValue, map)) {
                        return true;
                    } else {
                        ++currentIndex;
                        stack.Pop();
                        continue;
                    }
                }

                ++currentIndex;
            }

            return false;
        }

        /// <summary>
        /// Recurse through neighboring indices of some 2D structure, calling the given function with each index set
        /// </summary>
        /// <param name="validationFunc_">Function to call on each index set; return type: 
        ///   (TRUE): stop recursion entirely, (FALSE): stop recursing current index, (NULL): continue recursion</param>
        /// <param name="validateFirstStartingElement_">Whether to call the validation function on the first indices</param>
        /// <example>
        /// DepthFirstSearch_2D usage:
        /// Generally should involve some flag/counter field to mark indices already checked; example:
        ///
        ///     bool[,] array = new bool[10,10];
        ///     DepthFirstSearch_2D(5,5, (x,y) => {
        ///         if(x &lt; 0 || x &gt;= 10 || y &lt; 0 || y &gt;= 10) return false;
        ///         if(array[x,y]) return false;
        ///         array[x,y] = true;
        ///         return null;
        ///     });
        /// </example>
        public static void DepthFirstSearch_2D(int x_, int y_, System.Func<int,int,bool?> validationFunc_, bool validateFirstStartingElement_ = true, int depthLimit_ = int.MaxValue) {
            if(validateFirstStartingElement_ && validationFunc_(x_,y_).HasValue) return;
            _DepthFirstSearch_2D(x_, y_, validationFunc_, 0, depthLimit_);
        }

        private static bool _DepthFirstSearch_2D(int x_, int y_, System.Func<int,int,bool?> validationFunc_, int currDepth_ = 0, int depthLimit_ = int.MaxValue) {
            if(currDepth_ >= depthLimit_) return false;

            bool? res = null;
            res = validationFunc_(x_-1,y_);
            if(!res.HasValue) {
                if(_DepthFirstSearch_2D(x_-1,y_,validationFunc_, currDepth_ + 1)) return true;
            } else if(res.Value) return true;

            res = validationFunc_(x_+1,y_);
            if(!res.HasValue) {
                if(_DepthFirstSearch_2D(x_+1,y_,validationFunc_, currDepth_ + 1)) return true;
            } else if(res.Value) return true;
            
            res = validationFunc_(x_,y_-1);
            if(!res.HasValue) {
                if(_DepthFirstSearch_2D(x_,y_-1,validationFunc_, currDepth_ + 1)) return true;
            } else if(res.Value) return true;
            
            res = validationFunc_(x_,y_+1);
            if(!res.HasValue) {
                if(_DepthFirstSearch_2D(x_,y_+1,validationFunc_, currDepth_ + 1)) return true;
            } else if(res.Value) return true;

            return false;
        }

#if !MAX_COMPATIBILTY
        /// <summary>
        /// BFS search, with a validation function taking in the X-Y corridinates of each iteration
        /// </summary>
        public static void BreadthFirstSearch_2D(int x_, int y_, System.Func<int,int,bool?> validationFunc_, bool validateFirstStartingElement_ = true, int depthLimit_ = int.MaxValue) {
            BreadthFirstSearch_2D(x_,y_, (x,y,depth) => {
                return validationFunc_(x,y);
            }, validateFirstStartingElement_, depthLimit_);
        }

        /// <summary>
        /// BFS search, with a validation function taking in the X-Y corridinates AND the depth of each iteration
        /// </summary>
        public static void BreadthFirstSearch_2D(int x_, int y_, System.Func<int,int,int,bool?> validationFunc_, bool validateFirstStartingElement_ = true, int depthLimit_ = int.MaxValue) {
            //each tuple: <X,Y,Dist>
            Queue<ImmutableTuple<int,int,int>> queue = new Queue<ImmutableTuple<int,int,int>>();
            if(validateFirstStartingElement_) {
                bool? val = validationFunc_(x_,y_,0);
                if(val.HasValue) return;
            }

            queue.Enqueue(new ImmutableTuple<int,int,int>(x_+1,y_, 1));
            queue.Enqueue(new ImmutableTuple<int,int,int>(x_,y_+1, 1));
            queue.Enqueue(new ImmutableTuple<int,int,int>(x_-1,y_, 1));
            queue.Enqueue(new ImmutableTuple<int,int,int>(x_,y_-1, 1));

            while(queue.Count > 0) {
                var pair = queue.Dequeue();
                if(pair.Item3 >= depthLimit_) continue;
                bool? val = validationFunc_(pair.Item1, pair.Item2, pair.Item3);
                if(val.HasValue) {
                    if(val.Value) return;
                    else continue;
                }else{
                    queue.Enqueue(new ImmutableTuple<int,int,int>(pair.Item1+1, pair.Item2, pair.Item3 + 1));
                    queue.Enqueue(new ImmutableTuple<int,int,int>(pair.Item1, pair.Item2+1, pair.Item3 + 1));
                    queue.Enqueue(new ImmutableTuple<int,int,int>(pair.Item1-1, pair.Item2, pair.Item3 + 1));
                    queue.Enqueue(new ImmutableTuple<int,int,int>(pair.Item1, pair.Item2-1, pair.Item3 + 1));
                }
            }
        }

        /// <summary>
        /// Replaces list from some other list. Useful since it uses IList interface.
        /// </summary>
        public static void ReplaceFromOther<T>(this IList<T> list, IList<T> other)
        {
            list.Clear();
            for (int i = 0; i < other.Count; i++)
                list.Add(other[i]);
        }
#endif
        /// <summary>
        /// A sort for lists that doesn't reorder items that are equal.  An inefficient bubble sort at the moment.
        /// </summary>
        public static void StableSort<T>( IList<T> list, Func<T, T, int> comparer ) {
            bool swap = false;
            do {
                swap = false;
                for( int i = 0; i < list.Count - 1; i++ ) {
                    if( comparer( list[i], list[i + 1] ) == 1 ) {
                        T temp = list[i];
                        list[i] = list[i + 1];
                        list[i + 1] = temp;
                        swap = true;
                    }
                }
            } while( swap );
        }

        #region Join
#if !MAX_COMPATIBILTY
        ///// <summary>
        ///// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        ///// </summary>
        ///// <param name="joiner">The value to insert between entries of value.</param>
        ///// <param name="values">The collection to join into a string.</param>
        ///// <returns>The fully joined string.</returns>
        //public static string Join(string joiner, params object[] values)
        //{
        //    return Join(joiner, (IEnumerable<object>)values);
        //}

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join(string joiner, params string[] values)
        {
            return string.Join(joiner, values);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join(string joiner, IEnumerable values)
        {
            return Join(joiner, values, null);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join(string joiner, IEnumerable values, Func<object, string> converter)
        {
            if (values == null)
            {
                return "";
            }

            if (values is ICollection)
            {
                ICollection collection = (ICollection)values;
                int count = collection.Count;
                if (count == 0)
                {
                    return "";
                }

                if (count == 1)
                {
                    if (collection is IList)
                    {
                        return Join_Convert(((IList)collection)[0], converter);
                    }
                    else
                    {
                        var enumerator = collection.GetEnumerator();
                        try
                        {
                            if (enumerator.MoveNext())
                            {
                                return Join_Convert(enumerator.Current, converter);
                            }
                            else
                            {
                                return "";
                            }
                        }
                        finally
                        {
                            if (enumerator is IDisposable)
                            {
                                ((IDisposable)enumerator).Dispose();
                            }
                        }
                    }
                }
                else if (values is string[] && converter == null)
                {
                    return string.Join(joiner, (string[])values);
                }
            }

            using (Pooled<StringWriter> writer = RecyclingPool.SpawnStringWriter())
            {
                Join(writer.Value, joiner, values, converter);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join<TElement>(string joiner, IEnumerable<TElement> values)
        {
            return Join(joiner, values, null);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join<TElement>(string joiner, IEnumerable<TElement> values, Func<TElement, string> converter)
        {
            if (values == null)
            {
                return "";
            }

            if (values is ICollection<TElement>)
            {
                ICollection<TElement> collection = (ICollection<TElement>)values;
                int count = collection.Count;
                if (count == 0)
                {
                    return "";
                }

                if (count == 1)
                {
                    if (collection is IList<TElement>)
                    {
                        Join_Convert(((IList<TElement>)collection)[0], converter);
                    }
                    else
                    {
                        var enumerator = collection.GetEnumerator();
                        try
                        {
                            if (enumerator.MoveNext())
                            {
                                return Join_Convert(enumerator.Current, converter);
                            }
                            else
                            {
                                return "";
                            }
                        }
                        finally
                        {
                            enumerator.Dispose();
                        }
                    }
                }
                else if (values is string[] && converter == null)
                {
                    return string.Join(joiner, (string[])(Array)values);
                }
            }

            using (Pooled<StringWriter> writer = RecyclingPool.SpawnStringWriter())
            {
                Join(writer.Value, joiner, values, converter);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join(string joiner, IList values, int index, int count)
        {
            return Join(joiner, values, index, count, null);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join(string joiner, IList values, int index, int count, Func<object, string> converter)
        {
            if (values == null)
            {
                return "";
            }
            if (count < 0 || count > values.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0 || index + count > values.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count == 0)
            {
                return "";
            }
            else if (count == 1)
            {
                return Join_Convert(values[index], converter);
            }
            else if (values is string[] && converter == null)
            {
                return string.Join(joiner, (string[])values, index, count);
            }

            using (Pooled<StringWriter> writer = RecyclingPool.SpawnStringWriter())
            {
                Join(writer.Value, joiner, values, index, count, converter);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join<TElement>(string joiner, IList<TElement> values, int index, int count)
        {
            return Join(joiner, values, index, count, null);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static string Join<TElement>(string joiner, IList<TElement> values, int index, int count, Func<TElement, string> converter)
        {
            if (values == null)
            {
                return "";
            }
            if (count < 0 || count > values.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0 || index + count > values.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count == 0)
            {
                return "";
            }
            else if (count == 1)
            {
                return Join_Convert(values[index], converter);
            }
            else if (values is string[] && converter == null)
            {
                return string.Join(joiner, (string[])(Array)values, index, count);
            }

            using (Pooled<StringWriter> writer = RecyclingPool.SpawnStringWriter())
            {
                Join(writer.Value, joiner, values, index, count, converter);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Efficiently joins a set of strings, appending them to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        public static void Join(TextWriter writer, string joiner, IEnumerable values)
        {
            Join(writer, joiner, values, null);
        }

        /// <summary>
        /// Efficiently joins a set of strings, appending them to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static void Join(TextWriter writer, string joiner, IEnumerable values, Func<object, string> converter)
        {
            if (values == null)
            {
                return;
            }
            if (writer == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (values is IList)
            {
                IList list = (IList)values;
                Join(writer, joiner, list, 0, list.Count, converter);
            }
            else
            {
                bool first = true;
                // Interface enumerators always allocate, even when using GetEnumerator manually.
                foreach (object element in values)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.Write(joiner);
                    }
                    writer.Write(Join_Convert(element, converter));
                }
            }
        }

        /// <summary>
        /// Efficiently joins a set of strings, appending them to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        public static void Join<TElement>(TextWriter writer, string joiner, IEnumerable<TElement> values)
        {
            Join(writer, joiner, values, null);
        }

        /// <summary>
        /// Efficiently joins a set of strings, appending them to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static void Join<TElement>(TextWriter writer, string joiner, IEnumerable<TElement> values, Func<TElement, string> converter)
        {
            if (values == null)
            {
                return;
            }
            if (writer == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (values is IList<TElement>)
            {
                IList<TElement> list = (IList<TElement>)values;
                Join(writer, joiner, list, 0, list.Count, converter);
            }
            else
            {
                bool first = true;
                // Interface enumerators always allocate, even when using GetEnumerator manually.
                foreach (TElement element in values)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.Write(joiner);
                    }
                    writer.Write(Join_Convert(element, converter));
                }
            }
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="index">The starting index within the list.</param>
        /// <param name="count">The number of entries to join.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        public static void Join(TextWriter writer, string joiner, IList values, int index, int count)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (values == null)
                return;

            Join(writer, joiner, values, index, count, null);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="index">The starting index within the list.</param>
        /// <param name="count">The number of entries to join.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        public static void Join(TextWriter writer, string joiner, IList values, int index, int count, Func<object, string> converter)
        {
            if (values == null)
            {
                return;
            }
            if (count < 0 || count > values.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0 || index + count > values.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (count > 0)
            {
                writer.Write(Join_Convert(values[index], converter));
                for (int i = index + 1; i < count; ++i)
                {
                    writer.Write(joiner);
                    writer.Write(Join_Convert(values[i], converter));
                }
            }
        }

        /// <summary>
        /// Efficiently joins a set of strings, appending them to the given <see cref="TextWriter"/>.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="index">The starting index within the list.</param>
        /// <param name="count">The number of entries to join.</param>
        public static void Join<TElement>(TextWriter writer, string joiner, IList<TElement> values, int index, int count)
        {
            Join(writer, joiner, values, index, count, null);
        }

        /// <summary>
        /// Efficiently joins a set of objects into a string, using minimal allocations and returning the result.
        /// </summary>
        /// <typeparam name="TElement">The collection value type to join.</typeparam>
        /// <param name="writer">The <see cref="TextWriter"/> to append the string to.</param>
        /// <param name="joiner">The value to insert between entries of value.</param>
        /// <param name="values">The collection to join into a string.</param>
        /// <param name="index">The starting index within the list.</param>
        /// <param name="count">The number of entries to join.</param>
        /// <param name="converter">An optional conversion method applied to each element.</param>
        /// <returns>The fully joined string.</returns>
        public static void Join<TElement>(TextWriter writer, string joiner, IList<TElement> values, int index, int count, Func<TElement, string> converter)
        {
            if (values == null)
            {
                return;
            }
            if (count < 0 || count > values.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0 || index + count > values.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (writer == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (count > 0)
            {
                writer.Write(Join_Convert(values[index], converter));
                for (int i = index + 1; i < count; ++i)
                {
                    writer.Write(joiner);
                    writer.Write(Join_Convert(values[i], converter));
                }
            }
        }

        private static string Join_Convert<TElement>(TElement value, Func<TElement, string> converter)
        {
            if (converter != null)
            {
                return converter(value);
            }
            else if (typeof(TElement).IsValueType || !ReferenceEquals(value, null)) {
                return value.ToString();
            }
            else {
                return "";
            }
        }
#endif
#endregion

    }
}
