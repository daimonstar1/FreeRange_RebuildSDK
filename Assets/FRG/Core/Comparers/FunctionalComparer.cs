using System;
using System.Collections;
using System.Collections.Generic;

namespace FRG.Core
{
    /// <summary>
    /// A comparer that is based on a method.
    /// </summary>
    public class FunctionalComparer<T> : IComparer<T>
    {
        private Comparison<T> comparisonFunc;

        /// <summary>
        /// Creates a new functional comparer.
        /// </summary>
        /// <param name="comparison">The comparison to adapt.</param>
        public FunctionalComparer(Comparison<T> comparisonFunc)
        {
            if (comparisonFunc == null) throw new ArgumentNullException("comparisonFunc");

            this.comparisonFunc = comparisonFunc;
        }

        public int Compare(T left, T right)
        {
            return comparisonFunc(left, right);
        }
    }
}
