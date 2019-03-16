using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FRG.SharedCore
{

    public static class RandomUtil
    {
        public static T PickRandom<T>(this List<T> list)
        {
            if (list.Count == 0) return default(T);
            int index = Random.Range( 0, list.Count - 1 );
            return list[index];
        }

        public static List<T> PickRandom<T>(this List<T> list, int n)
        {
            if (list.Count <= n)
            {
                return new List<T>(list);
            }
            List<T> picked = new List<T>( n );
            int max = list.Count-1;
            for (int i = 0; i < n; i++)
            {
                int which = Random.Range( i, max );
                picked.Add(list[which]);
                list[which] = list[max];
                max--;
            }
            return picked;
        }

        public static T PickRandom<T>(this ISet<T> hashSet)
        {
            if (hashSet.Count == 0) return default(T);
            var enumerator = hashSet.GetEnumerator();
            int index = Random.Range(0, hashSet.Count - 1);
            // Call at least once
            for (; index >= 0; --index)
            {
                enumerator.MoveNext();
            }
            return enumerator.Current;
        }
    }
}
