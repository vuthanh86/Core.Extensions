using System;
using System.Collections.Generic;

namespace NetCore.Extensions.Core.Generics
{
    public sealed class SimpleEqualityComparer<T>:IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> comparer;
        private readonly Func<T, int> hashCodeFunc;

        private SimpleEqualityComparer(Func<T, T, bool> comparer, Func<T,int> hashCodeFunc = null)
        {
            this.comparer = comparer;
            this.hashCodeFunc = hashCodeFunc ?? (arg => arg.GetHashCode());
        }

        #region Implementation of IEqualityComparer<in T>

        public bool Equals(T x, T y)
        {
            return comparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return hashCodeFunc.Invoke(obj);
        }

        #endregion

        public static IEqualityComparer<T> Create(Func<T, T, bool> comparer, Func<T, int> hashCodeFunc = null)
        {
            return new SimpleEqualityComparer<T>(comparer, hashCodeFunc);
        }
    }
}
