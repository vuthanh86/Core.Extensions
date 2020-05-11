using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Core.Collections
{
    public interface IGroupDictionary<TGroup,TKey,TValue>
    {
        IDictionary<TKey, TValue> Get(TGroup group);
        bool TryGet(TGroup group, TKey key, out TValue value);
        TValue GetOrAdd(TGroup group, TKey key, Func<TGroup, TKey, TValue> factory);
    }

    public class GroupDictionary<TGroup, TKey, TValue> : IGroupDictionary<TGroup, TKey, TValue>
    {
        private readonly Dictionary<TGroup, IDictionary<TKey, TValue>> groups = new Dictionary<TGroup,IDictionary<TKey,TValue>>();

        IDictionary<TKey, TValue> GetOrAdd(TGroup groupKey)
        {
            IDictionary<TKey, TValue> g;
            if (!groups.TryGetValue(groupKey, out g))
            {
                g = new Dictionary<TKey, TValue>();
                groups[groupKey] = g;
            }

            return g;
        }

        public IDictionary<TKey, TValue> Get(TGroup @group)
        {
            return GetOrAdd(group);
        }

        public bool TryGet(TGroup @group, TKey key, out TValue value)
        {
            var g = GetOrAdd(group);
            return g.TryGetValue(key, out value);
        }

        public TValue GetOrAdd(TGroup @group, TKey key, Func<TGroup, TKey, TValue> factory)
        {
            var g = GetOrAdd(group);
            TValue value;
            if (!g.TryGetValue(key, out value))
            {
                value = factory(group, key);
                g[key] = value;
            }
            return value;
        }
    }

    public class LazyDictionary<TKey, TValue> : IDictionary<TKey,TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dic;

        public LazyDictionary()
        {
            _dic = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(_dic.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dic.TryAdd(item.Key, new Lazy<TValue>(() => item.Value));
        }

        public void Clear()
        {
            _dic.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dic.TryGetValue(item.Key, out var value) && value.Value.Equals(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        public int Count => _dic.Count;
        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IDictionary<TKey,TValue>

        public void Add(TKey key, TValue value)
        {
            _dic.TryAdd(key, new Lazy<TValue>(() => value));
        }

        public bool ContainsKey(TKey key)
        {
            return _dic.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return _dic.TryRemove(key, out _);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dic.TryGetValue(key, out var lazy))
            {
                value = lazy.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                    return value;

                throw new KeyNotFoundException();
            }
            set
            {
                _dic[key] = new Lazy<TValue>(() => value);
            }
        }

        public ICollection<TKey> Keys => _dic.Keys;

        public ICollection<TValue> Values
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            return _dic.GetOrAdd(key, x => new Lazy<TValue>(()=>factory(x))).Value;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (_dic.TryRemove(key, out var lz))
            {
                value = lz.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return _dic.TryAdd(key, new Lazy<TValue>(() => value));
        }

        public void AddOrUpdate(TKey key, TValue value, Func<TKey,TValue, TValue> factory)
        {
            _dic.AddOrUpdate(key, new Lazy<TValue>(() => value), (x, old) => new Lazy<TValue>(()=>factory(x, old.Value)));
        }

        class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly IEnumerator<KeyValuePair<TKey, Lazy<TValue>>> _e;

            public Enumerator(IEnumerator<KeyValuePair<TKey,Lazy<TValue>>> e)
            {
                _e = e;
            }

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            public void Reset()
            {
                _e.Reset();
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_e.Current.Key, _e.Current.Value.Value);

            object IEnumerator.Current => Current;

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                _e.Dispose();
            }

            #endregion
        }
    }
}
