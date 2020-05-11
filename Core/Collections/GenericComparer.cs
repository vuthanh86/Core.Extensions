using System;
using System.Collections.Generic;

namespace Core.Collections
{
    public class GenericComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> comparer;

        public GenericComparer(Func<T, T, int> comparer)
        {
            this.comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return comparer(x, y);
        }
    }
}
