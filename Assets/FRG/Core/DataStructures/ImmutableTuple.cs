using System;
using System.Collections;
using System.Collections.Generic;

namespace FRG.Core
{
    // Unity3D doesn't like these static methods
    ///// <summary>
    ///// Static methods for an immutable tuple struct that can be used as a hash or dictionary key.
    ///// </summary>
    //public static class ImmutableTuple
    //{

    //    /// <summary>
    //    /// Creates a new key tuple.
    //    /// </summary>
    //    /// <typeparam name="T1">A tuple member type.</typeparam>
    //    /// <param name="item1">A tuple member item.</param>
    //    /// <returns>The new key tuple.</returns>
    //    public static ImmutableTuple<T1> Create<T1>(T1 item1)
    //    {
    //        return new ImmutableTuple<T1>(item1);
    //    }

    //    /// <summary>
    //    /// Creates a new key tuple.
    //    /// </summary>
    //    /// <typeparam name="T1">A tuple member type.</typeparam>
    //    /// <typeparam name="T2">A tuple member type.</typeparam>
    //    /// <param name="item1">A tuple member item.</param>
    //    /// <param name="item2">A tuple member item.</param>
    //    /// <returns>The new key tuple.</returns>
    //    public static ImmutableTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
    //    {
    //        return new ImmutableTuple<T1, T2>(item1, item2);
    //    }

    //    /// <summary>
    //    /// Creates a new key tuple.
    //    /// </summary>
    //    /// <typeparam name="T1">A tuple member type.</typeparam>
    //    /// <typeparam name="T2">A tuple member type.</typeparam>
    //    /// <typeparam name="T3">A tuple member type.</typeparam>
    //    /// <param name="item1">A tuple member item.</param>
    //    /// <param name="item2">A tuple member item.</param>
    //    /// <param name="item3">A tuple member item.</param>
    //    /// <returns>The new key tuple.</returns>
    //    public static ImmutableTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
    //    {
    //        return new ImmutableTuple<T1, T2, T3>(item1, item2, item3);
    //    }

    //    /// <summary>
    //    /// Creates a new key tuple.
    //    /// </summary>
    //    /// <typeparam name="T1">A tuple member type.</typeparam>
    //    /// <typeparam name="T2">A tuple member type.</typeparam>
    //    /// <typeparam name="T3">A tuple member type.</typeparam>
    //    /// <typeparam name="T4">A tuple member type.</typeparam>
    //    /// <param name="item1">A tuple member item.</param>
    //    /// <param name="item2">A tuple member item.</param>
    //    /// <param name="item3">A tuple member item.</param>
    //    /// <param name="item4">A tuple member item.</param>
    //    /// <returns>The new key tuple.</returns>
    //    public static ImmutableTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
    //    {
    //        return new ImmutableTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
    //    }

    //    /// <summary>
    //    /// Creates a new key tuple.
    //    /// </summary>
    //    /// <typeparam name="T1">A tuple member type.</typeparam>
    //    /// <typeparam name="T2">A tuple member type.</typeparam>
    //    /// <typeparam name="T3">A tuple member type.</typeparam>
    //    /// <typeparam name="T4">A tuple member type.</typeparam>
    //    /// <typeparam name="T5">A tuple member type.</typeparam>
    //    /// <param name="item1">A tuple member item.</param>
    //    /// <param name="item2">A tuple member item.</param>
    //    /// <param name="item3">A tuple member item.</param>
    //    /// <param name="item4">A tuple member item.</param>
    //    /// <param name="item5">A tuple member item.</param>
    //    /// <returns>The new key tuple.</returns>
    //    public static ImmutableTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
    //    {
    //        return new ImmutableTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
    //    }
    //}

    /// <summary>
    /// Provides a method for converting to array.
    /// </summary>
    public interface IArrayConvertible
    {
        /// <summary>
        /// Converts this object to object array.
        /// </summary>
        /// <returns>This object expressed as an object array.</returns>
        object[] ToArray();
    }

    /// <summary>
    /// A tuple struct that only contains immutable data.
    /// </summary>
    [Serializable]
    //[StructLayout(LayoutKind.Auto)]
    public struct ImmutableTuple<T1> : IEquatable<ImmutableTuple<T1>>, IArrayConvertible
    {
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T1 Item1;

        /// <summary>
        /// The only item in the single-value tuple.
        /// </summary>
        public T1 Item
        {
            get
            {
                return Item1;
            }
        }

        /// <summary>
        /// Creates a new key tuple.
        /// </summary>
        /// <param name="item1">A tuple member.</param>
        public ImmutableTuple(T1 item1)
        {
            Item1 = item1;
        }

        /// <summary>
        /// Compare two key tuples for equality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==(ImmutableTuple<T1> a, ImmutableTuple<T1> b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compare two key tuples for inequality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=(ImmutableTuple<T1> a, ImmutableTuple<T1> b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public bool Equals(ImmutableTuple<T1> obj)
        {
            return SafeEqualityComparer<T1>.Default.Equals(Item1, obj.Item1);
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImmutableTuple<T1>))
            {
                return false;
            }
            return (this == (ImmutableTuple<T1>)obj);
        }

        /// <summary>
        /// Gets a hash code for this tuple.
        /// </summary>
        /// <returns>The hash code for this tuple.</returns>
        public override int GetHashCode()
        {
            return SafeEqualityComparer<T1>.Default.GetHashCode(Item1);
        }

        /// <summary>
        /// Converts this tuple to object array.
        /// </summary>
        /// <returns>This tuple expressed as an object array.</returns>
        public object[] ToArray()
        {
            return new object[] { Item1 };
        }

        /// <summary>
        /// Converts this request to a string representation.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return "ImmutableTuple(" + Item1 + ")";
        }
    }

    /// <summary>
    /// A tuple struct that only contains immutable data.
    /// </summary>
    [Serializable]
    //[StructLayout(LayoutKind.Auto)]
    public struct ImmutableTuple<T1, T2> : IEquatable<ImmutableTuple<T1, T2>>, IArrayConvertible
    {

        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T1 Item1;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T2 Item2;

        /// <summary>
        /// Creates a new key tuple.
        /// </summary>
        /// <param name="item1">A tuple member.</param>
        /// <param name="item2">A tuple member.</param>
        public ImmutableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// Compare two key tuples for equality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==(ImmutableTuple<T1, T2> a, ImmutableTuple<T1, T2> b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compare two key tuples for inequality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=(ImmutableTuple<T1, T2> a, ImmutableTuple<T1, T2> b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public bool Equals(ImmutableTuple<T1, T2> obj)
        {
            return (SafeEqualityComparer<T1>.Default.Equals(Item1, obj.Item1) &&
                SafeEqualityComparer<T2>.Default.Equals(Item2, obj.Item2));
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImmutableTuple<T1, T2>))
            {
                return false;
            }
            return (this == (ImmutableTuple<T1, T2>)obj);
        }

        /// <summary>
        /// Gets a hash code for this tuple.
        /// </summary>
        /// <returns>The hash code for this tuple.</returns>
        public override int GetHashCode()
        {
            int hash = SafeEqualityComparer<T1>.Default.GetHashCode(Item1);
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T2>.Default.GetHashCode(Item2));
            return hash;
        }

        /// <summary>
        /// Converts this tuple to object array.
        /// </summary>
        /// <returns>This tuple expressed as an object array.</returns>
        public object[] ToArray()
        {
            return new object[] { Item1, Item2 };
        }

        /// <summary>
        /// Converts this request to a string representation.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return "ImmutableTuple(" + Item1 + ", " + Item2 + ")";
        }
    }

    /// <summary>
    /// A tuple struct that only contains immutable data.
    /// </summary>
    [Serializable]
    //[StructLayout(LayoutKind.Auto)]
    public struct ImmutableTuple<T1, T2, T3> : IEquatable<ImmutableTuple<T1, T2, T3>>, IArrayConvertible
    {

        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T1 Item1;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T2 Item2;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T3 Item3;

        /// <summary>
        /// Creates a new key tuple.
        /// </summary>
        /// <param name="item1">A tuple member.</param>
        /// <param name="item2">A tuple member.</param>
        /// <param name="item3">A tuple member.</param>
        public ImmutableTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        /// <summary>
        /// Compare two key tuples for equality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==(ImmutableTuple<T1, T2, T3> a, ImmutableTuple<T1, T2, T3> b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compare two key tuples for inequality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=(ImmutableTuple<T1, T2, T3> a, ImmutableTuple<T1, T2, T3> b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public bool Equals(ImmutableTuple<T1, T2, T3> obj)
        {
            return (SafeEqualityComparer<T1>.Default.Equals(Item1, obj.Item1) &&
                SafeEqualityComparer<T2>.Default.Equals(Item2, obj.Item2) &&
                SafeEqualityComparer<T3>.Default.Equals(Item3, obj.Item3));
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImmutableTuple<T1, T2, T3>))
            {
                return false;
            }
            return (this == (ImmutableTuple<T1, T2, T3>)obj);
        }

        /// <summary>
        /// Gets a hash code for this tuple.
        /// </summary>
        /// <returns>The hash code for this tuple.</returns>
        public override int GetHashCode()
        {
            int hash = SafeEqualityComparer<T1>.Default.GetHashCode(Item1);
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T2>.Default.GetHashCode(Item2));
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T3>.Default.GetHashCode(Item3));
            return hash;
        }

        /// <summary>
        /// Converts this tuple to object array.
        /// </summary>
        /// <returns>This tuple expressed as an object array.</returns>
        public object[] ToArray()
        {
            return new object[] { Item1, Item2, Item3 };
        }

        /// <summary>
        /// Converts this request to a string representation.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return "ImmutableTuple(" + Item1 + ", " + Item2 + ", " + Item3 + ")";
        }
    }

    /// <summary>
    /// A tuple struct that only contains immutable data.
    /// </summary>
    [Serializable]
    //[StructLayout(LayoutKind.Auto)]
    public struct ImmutableTuple<T1, T2, T3, T4> : IEquatable<ImmutableTuple<T1, T2, T3, T4>>, IArrayConvertible
    {

        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T1 Item1;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T2 Item2;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T3 Item3;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T4 Item4;

        /// <summary>
        /// Creates a new key tuple.
        /// </summary>
        /// <param name="item1">A tuple member.</param>
        /// <param name="item2">A tuple member.</param>
        /// <param name="item3">A tuple member.</param>
        /// <param name="item4">A tuple member.</param>
        public ImmutableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        /// <summary>
        /// Compare two key tuples for equality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==(ImmutableTuple<T1, T2, T3, T4> a, ImmutableTuple<T1, T2, T3, T4> b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compare two key tuples for inequality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=(ImmutableTuple<T1, T2, T3, T4> a, ImmutableTuple<T1, T2, T3, T4> b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public bool Equals(ImmutableTuple<T1, T2, T3, T4> obj)
        {
            return (SafeEqualityComparer<T1>.Default.Equals(Item1, obj.Item1) &&
                SafeEqualityComparer<T2>.Default.Equals(Item2, obj.Item2) &&
                SafeEqualityComparer<T3>.Default.Equals(Item3, obj.Item3) &&
                SafeEqualityComparer<T4>.Default.Equals(Item4, obj.Item4));
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImmutableTuple<T1, T2, T3, T4>))
            {
                return false;
            }
            return (this == (ImmutableTuple<T1, T2, T3, T4>)obj);
        }

        /// <summary>
        /// Gets a hash code for this tuple.
        /// </summary>
        /// <returns>The hash code for this tuple.</returns>
        public override int GetHashCode()
        {
            int hash = SafeEqualityComparer<T1>.Default.GetHashCode(Item1);
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T2>.Default.GetHashCode(Item2));
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T3>.Default.GetHashCode(Item3));
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T4>.Default.GetHashCode(Item4));
            return hash;
        }

        /// <summary>
        /// Converts this tuple to object array.
        /// </summary>
        /// <returns>This tuple expressed as an object array.</returns>
        public object[] ToArray()
        {
            return new object[] { Item1, Item2, Item3, Item4 };
        }

        /// <summary>
        /// Converts this request to a string representation.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return "ImmutableTuple(" + Item1 + ", " + Item2 + ", " + Item3 + ", " + Item4 + ")";
        }
    }

    /// <summary>
    /// A tuple struct that only contains immutable data.
    /// </summary>
    [Serializable]
    //[StructLayout(LayoutKind.Auto)]
    public struct ImmutableTuple<T1, T2, T3, T4, T5> : IEquatable<ImmutableTuple<T1, T2, T3, T4, T5>>, IArrayConvertible
    {

        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T1 Item1;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T2 Item2;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T3 Item3;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T4 Item4;
        /// <summary>
        /// A tuple member.
        /// </summary>
        public readonly T5 Item5;

        /// <summary>
        /// Creates a new key tuple.
        /// </summary>
        /// <param name="item1">A tuple member.</param>
        /// <param name="item2">A tuple member.</param>
        /// <param name="item3">A tuple member.</param>
        /// <param name="item4">A tuple member.</param>
        /// <param name="item5">A tuple member.</param>
        public ImmutableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

        /// <summary>
        /// Compare two key tuples for equality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==(ImmutableTuple<T1, T2, T3, T4, T5> a, ImmutableTuple<T1, T2, T3, T4, T5> b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compare two key tuples for inequality.
        /// </summary>
        /// <param name="a">A key tuple.</param>
        /// <param name="b">Another key tuple.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=(ImmutableTuple<T1, T2, T3, T4, T5> a, ImmutableTuple<T1, T2, T3, T4, T5> b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public bool Equals(ImmutableTuple<T1, T2, T3, T4, T5> obj)
        {
            return (SafeEqualityComparer<T1>.Default.Equals(Item1, obj.Item1) &&
                SafeEqualityComparer<T2>.Default.Equals(Item2, obj.Item2) &&
                SafeEqualityComparer<T3>.Default.Equals(Item3, obj.Item3) &&
                SafeEqualityComparer<T4>.Default.Equals(Item4, obj.Item4) &&
                SafeEqualityComparer<T5>.Default.Equals(Item5, obj.Item5));
        }

        /// <summary>
        /// Compares against another object for equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ImmutableTuple<T1, T2, T3, T4, T5>))
            {
                return false;
            }
            return (this == (ImmutableTuple<T1, T2, T3, T4, T5>)obj);
        }

        /// <summary>
        /// Gets a hash code for this tuple.
        /// </summary>
        /// <returns>The hash code for this tuple.</returns>
        public override int GetHashCode()
        {
            int hash = SafeEqualityComparer<T1>.Default.GetHashCode(Item1);
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T2>.Default.GetHashCode(Item2));
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T3>.Default.GetHashCode(Item3));
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T4>.Default.GetHashCode(Item4));
            hash = BitUtil.CombineHashCodes(hash, SafeEqualityComparer<T5>.Default.GetHashCode(Item5));
            return hash;
        }

        /// <summary>
        /// Converts this tuple to object array.
        /// </summary>
        /// <returns>This tuple expressed as an object array.</returns>
        public object[] ToArray()
        {
            return new object[] { Item1, Item2, Item3, Item4, Item5 };
        }

        /// <summary>
        /// Converts this request to a string representation.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            return "ImmutableTuple(" + Item1 + ", " + Item2 + ", " + Item3 + ", " + Item4 + ", " + Item5 + ")";
        }
    }
}