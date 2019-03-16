using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FRG.Core
{
    public static class SafeEqualityComparer<T>
    {
        /// <summary>
        /// A UnityEngine.Object comparer needed because these callbacks run on a different thread.
        /// </summary>
        /// <typeparam name="T">Reference types only, please.</typeparam>
        private class CheckedComparer : IEqualityComparer<T>
        {
            IEqualityComparer<T> _baseComparer = EqualityComparer<T>.Default;

            public bool Equals(T left, T right)
            {
                if (left is UnityEngine.Object)
                {
                    return ReferenceEquals(left, right);
                }
                else
                {
                    return _baseComparer.Equals(left, right);
                }
            }

            public int GetHashCode(T value)
            {
                // Null check
                if (ReferenceEquals(value, null))
                {
                    return 0;
                }

                // Hash codes should always be thread safe
                return _baseComparer.GetHashCode(value);
            }
        }

        /// <summary>
        /// A UnityEngine.Object comparer needed because these callbacks run on a different thread.
        /// </summary>
        /// <typeparam name="T">Reference types only, please.</typeparam>
        private class SafeComparer : IEqualityComparer<T>
        {
            IEqualityComparer<T> _baseComparer = EqualityComparer<T>.Default;

            public SafeComparer(IEqualityComparer<T> baseComparer)
            {
                _baseComparer = baseComparer;
            }

            public bool Equals(T left, T right)
            {
                return _baseComparer.Equals(left, right);
            }

            public int GetHashCode(T value)
            {
                // Null check
                if (ReferenceEquals(value, null))
                {
                    return 0;
                }

                // Hash codes should always be thread safe
                return value.GetHashCode();
            }
        }

        private static IEqualityComparer<T> _default = null;
        public static IEqualityComparer<T> Default
        {
            get
            {
                if (_default == null)
                {
#if !GAME_SERVER
                    Type genericType = typeof(T);
                    if (!genericType.IsValueType && (typeof(UnityEngine.Object).IsAssignableFrom(genericType) || genericType.IsInterface))
                    {
                        // Turns out unity objects are referentially equal
                        _default = new CheckedComparer();
                    }
                    else if (genericType == typeof(string))
                    {
                        _default = new SafeComparer((IEqualityComparer<T>)StringComparer.Ordinal);
                    }
                    else if (!genericType.IsValueType)
                    {
                        _default = new SafeComparer(EqualityComparer<T>.Default);
                    }
                    else
                    {
                        _default = EqualityComparer<T>.Default;
                    }
#else
                    // Threading may have this line called multiple times; that's OK.
                    _default = EqualityComparer<T>.Default;
#endif
                }
                return _default;
            }
        }
    }
}
