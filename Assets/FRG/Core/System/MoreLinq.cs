using FRG.SharedCore;
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq {

    /// <summary>
    /// System.Linq extensions class
    /// </summary>
    public static class MoreLinq {

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Comparison<TKey> comparison) {
            return source.OrderBy(keySelector, new ComparisonToComparer<TKey>(comparison));
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, Comparison<TSource> comparison) {
            return source.OrderBy(val => val, comparison);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Comparison<TKey> comparison) {
            return source.OrderByDescending(keySelector, new ComparisonToComparer<TKey>(comparison));
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> source, Comparison<TSource> comparison) {
            return source.OrderByDescending(val => val, comparison);
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, Comparison<TSource> comparison) {
            return source.Distinct(new ComparisonToComparer<TSource>(comparison));
        }

        private class ComparisonToComparer<T> : IComparer<T>, IEqualityComparer<T> {
            private Comparison<T> comp;

            public ComparisonToComparer(Comparison<T> comp) {
                this.comp = comp 
                    ?? ((a,b) => Comparer<T>.Default.Compare(a, b));
            }

            public int Compare(T a, T b) {
                return comp.Invoke(a, b);
            }

            public bool Equals(T a, T b) {
                return comp.Invoke(a, b) == 0;
            }

            public int GetHashCode(T a) {
                if(a == null) return 0;
                return a.GetHashCode();
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, Func<T,bool> additionalValues, params T[] additionalValueSelector) {
            if(source == null) throw new ArgumentNullException("source");
            
            var itor = source.GetEnumerator();
            while(itor.MoveNext()) {
                yield return itor.Current;
            }

            if(additionalValueSelector != null && additionalValues != null) {
                for(int i = 0; i<additionalValueSelector.Length; ++i) {
                    if(additionalValues.Invoke(additionalValueSelector[i])) {
                        yield return additionalValueSelector[i];
                    }
                }
            }
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, IEnumerable<T> additionalValues, Func<T, bool> additionalValueSelector) {
            if(source == null) throw new ArgumentNullException("source");
            
            var itor = source.GetEnumerator();
            while(itor.MoveNext()) {
                yield return itor.Current;
            }

            if(additionalValues != null && additionalValueSelector != null) {
                var itor2 = additionalValues.GetEnumerator();
                while(itor2.MoveNext()) {
                    if(additionalValueSelector.Invoke(itor2.Current)) {
                        yield return itor2.Current;
                    }
                }
            }
        }

        public static IEnumerator<T> EmptyEnumerator<T>() {
            yield break;
        }

        public static bool Contains(this IEnumerable enumerable, object val, IEqualityComparer comparer) {
            if(comparer == null) comparer = EqualityComparer<object>.Default;

            //special case for strings
            if((enumerable is string) && (val is string) && (comparer is IEqualityComparer<string>)) {
                return Contains((string)enumerable, (string)val, (IEqualityComparer<string>)comparer);
            }

            var itor = enumerable.GetEnumerator();
            while(itor.MoveNext()) {
                if(comparer.Equals(itor.Current, val)) return true;
            }

            return false;
        }

        public static bool Contains(this string str, string value, IEqualityComparer<string> comparer) {

            if(string.IsNullOrEmpty(str)) {
                return string.IsNullOrEmpty(value);
            }

            if(string.IsNullOrEmpty(value)) {
                return true;
            }

            //look for pre-defined comparisons and compare that way
            for(var itor = _stringComparerMap.GetEnumerator(); itor.MoveNext();) {
                if(itor.Current.Value == comparer) {
                    return str.IndexOf(value, itor.Current.Key) >= 0;
                }
            }

            //if this is a custom comparer, compare characters individually (SLOW)
            for(int i = 0; i<str.Length-value.Length+1; ++i) {
                bool match = true;
                for(int j = 0; j<value.Length; ++j) {
                    if(!comparer.Equals(str[i+j].ToString(), value[j].ToString())) {
                        match = false;
                        break;
                    }
                }

                if(match) return true;
            }

            return false;
        }

        static Dictionary<StringComparison, StringComparer> _stringComparerMap = new Dictionary<StringComparison, System.StringComparer> {
            { StringComparison.CurrentCulture, StringComparer.CurrentCulture },
            { StringComparison.CurrentCultureIgnoreCase, StringComparer.CurrentCultureIgnoreCase },
            { StringComparison.InvariantCulture, StringComparer.InvariantCulture },
            { StringComparison.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase },
            { StringComparison.Ordinal, StringComparer.Ordinal },
            { StringComparison.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase }
        };
    }
}
