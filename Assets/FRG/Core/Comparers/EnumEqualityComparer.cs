
using System;
using System.Collections.Generic;

namespace FRG.Core
{
#if NET_4_6
    /// <summary>
    /// This comparer uses .GetHashCode to compare enums and that shouldn't box on .NET 4.6 (mono)
    /// NOTE: This will not work in 2.0!
    /// </summary>
    public static class EnumEqualityComparer<T>
        where T : struct, IComparable
    {
        public static readonly IEqualityComparer<T> Default = new EqualityComparerImplementation();

        private class EqualityComparerImplementation : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return x.GetHashCode() == y.GetHashCode();
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }
    }
#else
    /// <summary>
    /// This comparer uses a field offset hack to cast generic enums to int without boxing on .NET 2.0 (mono)
    /// NOTE: This will not work in 4.6!
    /// </summary>
    public static class EnumEqualityComparer<T>
        where T : struct, IComparable
    {
        public static readonly IEqualityComparer<T> Default = new EqualityComparerImplementation();

        private class EqualityComparerImplementation : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return ToInt32(x) == ToInt32(y);
            }

            public int GetHashCode(T obj)
            {
                return ToInt32(obj);
            }

            private int ToInt32(T value)
            {
                EnumCastHack hack = new EnumCastHack();
                hack.EnumValue = value;
                return hack.IntValue;
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct EnumCastHack
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public int IntValue;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public T EnumValue;
        }
    }
#endif
}