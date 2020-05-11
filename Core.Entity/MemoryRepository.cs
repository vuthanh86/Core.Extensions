using Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Core.Entity
{
    public class MemoryRepository<T, TKey> : IRepository<T, TKey> where T : class, IBaseEntity<TKey>
    {
        private readonly ConcurrentDictionary<TKey, T> records = new ConcurrentDictionary<TKey, T>();

        #region Count

        public DbContext OpenConnection()
        {
            throw new NotSupportedException();
        }

        public int Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return records.Count;
            }

            var func = predicate.Compile();
            return records.Count(x => func(x.Value));
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return Task.FromResult(Count(predicate));
        }

        public long LongCount(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return records.Count;
            }

            var func = predicate.Compile();
            return records.LongCount(x => func(x.Value));
        }

        public Task<long> LongCountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return Task.FromResult(LongCount(predicate));
        }

        #endregion Count

        #region Delete

        public void Delete(T entity)
        {
            records.TryRemove(entity.Id, out _);
        }

        public Task DeleteAsync(T entity)
        {
            return Task.Run(() => { Delete(entity); });
        }

        public void DeleteMany(Expression<Func<T, bool>> filterExpression)
        {
            if (filterExpression == null)
            {
                records.Clear();
                return;
            }

            var func = filterExpression.Compile();
            var removed = new List<TKey>();

            foreach (var entry in records)
            {
                if (func(entry.Value))
                    removed.Add(entry.Key);
            }

            foreach (var id in removed)
            {
                records.TryRemove(id, out _);
            }
        }

        public void DeleteMany(IEnumerable<T> items)
        {
            var ids = items.Select(x => x.Id).ToArray();
            DeleteMany(r => ids.Contains(r.Id));
        }

        public Task DeleteManyAsync(Expression<Func<T, bool>> filterExpression)
        {
            return Task.Run(() => DeleteMany(filterExpression));
        }

        public Task DeleteManyAsync(IEnumerable<T> items)
        {
            return Task.Run(() => DeleteMany(items));
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null, params Expression<Func<T, dynamic>>[] includes)
        {
            if (includes.Length > 0)
                throw new NotSupportedException();

            return Find(filterExpression, findOptions);
        }

        #endregion Delete

        #region Find

        public IEnumerable<T> Find(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null)
        {
            var func = filterExpression?.Compile() ?? (t => true);

            if (findOptions == null)
                findOptions = new FindOptions<T>();

            var skip = findOptions.Skip ?? 0;
            var take = findOptions.Limit ?? records.Count;
            var found = records.Values.Where(func).Skip(skip).Take(take);

            return found;
        }

        public IEnumerable<TProjection> Find<TProjection>(Expression<Func<T, bool>> filterExpression, Expression<Func<T, TProjection>> projection, FindOptions<T> findOptions = null)
        {
            var func = projection.Compile();
            return Find(filterExpression, findOptions).Select(x => func(x));
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null)
        {
            return Task.Run(() => Find(filterExpression, findOptions));
        }

        public Task<IEnumerable<TProjection>> FindAsync<TProjection>(Expression<Func<T, bool>> filterExpression, Expression<Func<T, TProjection>> projection, FindOptions<T> findOptions = null)
        {
            return Task.Run(() => Find(filterExpression, projection, findOptions));
        }

        public T FindOne(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null, params Expression<Func<T, dynamic>>[] includes)
        {
            if (includes == null || includes.Length == 0)
            {
                return FindOne(filterExpression, findOptions);
            }
            throw new NotImplementedException();
        }

        public T FindOne(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null)
        {
            if (filterExpression == null)
                return records.Select(x => x.Value).FirstOrDefault();

            var func = filterExpression.Compile();

            foreach (var entry in records)
            {
                if (func(entry.Value)) return entry.Value;
            }

            return null;
        }

        public Task<T> FindOneAsync(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null)
        {
            return Task.Run(() => FindOne(filterExpression, findOptions));
        }

        public async Task<TProjection> FindOneAsync<TProjection>(Expression<Func<T, bool>> filterExpression, Expression<Func<T, TProjection>> projection)
        {
            var record = await FindOneAsync(filterExpression);
            if (record == null) return default(TProjection);

            return projection.Compile()(record);
        }

        public T GetById(TKey id)
        {
            if (records.TryGetValue(id, out T e))
                return e;
            return null;
        }

        public Task<T> GetByIdAsync(TKey id)
        {
            return Task.Run(() => GetById(id));
        }

        #endregion Find

        #region Insert

        public void Insert(T entity)
        {
            records.TryAdd(entity.Id, entity);
        }

        public Task InsertAsync(T entity)
        {
            return Task.Run(() => Insert(entity));
        }

        public void InsertMany(IEnumerable<T> items)
        {
            foreach (var item in items)
                Insert(item);
        }

        public Task InsertManyAsync(IEnumerable<T> items)
        {
            return Task.Run(() => InsertMany(items));
        }

        #endregion Insert

        #region Update

        public void Update(T entity)
        {
            records.AddOrUpdate(entity.Id, entity, (k, old) => entity);
        }

        public Task UpdateAsync(T entity)
        {
            return Task.Run(() => Update(entity));
        }

        public void UpdateMany(IEnumerable<T> items)
        {
            foreach (var item in items)
                Update(item);
        }

        public void UpdateMany(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression)
        {
            var filter = filterExpression?.Compile() ?? (t => true);
            var factory = updateExpression.Compile();

            foreach (var entry in records)
            {
                var r = entry.Value;
                if (!filter(r)) continue;
                Update(factory(r));
            }
        }

        public Task UpdateManyAsync(IEnumerable<T> items)
        {
            return Task.Run(() => UpdateMany(items));
        }

        public Task UpdateManyAsync(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression)
        {
            return Task.Run(() => UpdateMany(filterExpression, updateExpression));
        }

        #endregion Update
    }
    
    //public class CachedRepository<T> : ICachedRepository<T>
    //{
    //    private readonly ConcurrentDictionary<string, LazyItem<T>> concurrentDictionary = new ConcurrentDictionary<string, LazyItem<T>>();
    //    private readonly ConcurrentQueue<LazyItem<T>> expirationsQueue = new ConcurrentQueue<LazyItem<T>>();

    //    private readonly IScheduler scheduler;
       
    //    private readonly ILazyBuilder<T> lazyBuilder;
    //    private readonly CacheConfiguration config;

    //    public CachedRepository(ICacheFactory cacheFactory, IScheduler scheduler,
    //        ICacheSynchronizerFactory synchronizerFactory, IOptions<CacheConfiguration> config)
    //    {
    //        this.scheduler = scheduler;
    //        this.config = config.Value;

    //        var regionValue = typeof(T).GetRegionValue();

    //        cacheStorage = cacheFactory.CreateCacheStorage(new CacheStorageConfig
    //        {
    //            Prefix = string.IsNullOrEmpty(this.config.Prefix) ? regionValue : $"{this.config.Prefix}:{regionValue}",
    //            Database = this.config.RedisDatabase
    //        });

    //        synchronizer = synchronizerFactory.Create(regionValue);
    //        synchronizer.OnUpdated += OnUpdated;

    //        lazyBuilder = new LazyBuilder<T>(SyncFactory, AsyncFactory);
    //        ExpiredItemCleanerCallback();
    //    }

    //    public int InMemoryItemsCount => concurrentDictionary.Values.Count;

    //    private void ExpiredItemCleanerCallback()
    //    {
    //        scheduler.Schedule(obj => CleanExpiredValue(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    //    }

    //    private void OnUpdated(string[] keys)
    //    {
    //        if (keys.IsNullOrEmpty())
    //        {
    //            concurrentDictionary.Clear();
    //            expirationsQueue.Clear();
    //        }
    //        else
    //        {
    //            foreach (var k in keys)
    //            {
    //                ExpiredValue(k);
    //            }
    //        }
    //    }

    //    private LazyItem<T> Build(string key, Func<Task<T>> asyncFac = null, Func<T> syncFac = null)
    //    {
    //        var item = lazyBuilder.Build(key, syncFac, asyncFac);

    //        expirationsQueue.Enqueue(item);

    //        return item;
    //    }

    //    private LazyItem<T> Build(string key, T value)
    //    {
    //        return Build(key, () => Task.FromResult(value), () => value);
    //    }

    //    private void CleanExpiredValue()
    //    {
    //        var expiredKeys = new List<string>();
    //        var now = DateTimeOffset.UtcNow;

    //        while (expirationsQueue.TryPeek(out var p))
    //        {
    //            if (p.IsDead)
    //            {
    //                expirationsQueue.TryDequeue(out _);
    //                continue;
    //            }

    //            if ((p.CreateAt + config.MemoryTtl) <= now)
    //            {
    //                expiredKeys.Add(p.Key);
    //                expirationsQueue.TryDequeue(out _);
    //                //expired
    //            }
    //            else
    //            {
    //                break;
    //                //not yet expired
    //            }
    //        }

    //        foreach (var key in expiredKeys)
    //        {
    //            ExpiredValue(key);
    //        }

    //        if (expiredKeys.Count > 0)
    //            synchronizer.NotifyUpdate(expiredKeys.ToArray());
    //    }

    //    private void ExpiredValue(string key)
    //    {
    //        if (concurrentDictionary.TryRemove(key, out var lazyItem))
    //        {
    //            lazyItem.IsDead = true;
    //        }
    //    }

    //    private T SyncFactory(string key)
    //    {
    //        return cacheStorage.Get<T>(key).Value;
    //    }

    //    private async Task<T> AsyncFactory(string key)
    //    {
    //        var records = await cacheStorage.GetAsync<T>(key);
    //        return records.Value;
    //    }

    //    public T Get(string key)
    //    {
    //        return concurrentDictionary.GetOrAdd(key, s => Build(key)).ValueEntry.Value;
    //    }

    //    public Task<T> GetAsync(string key)
    //    {
    //        return concurrentDictionary.GetOrAdd(key, s => Build(key)).TaskEntry.Value;
    //    }

    //    public T GetOrAdd(string key, Func<T> valueFactory)
    //    {
    //        T GetFromOuterFactory()
    //        {
    //            var itemFromRedis = cacheStorage.Get<T>(key);

    //            if (itemFromRedis.Value != null)
    //                return itemFromRedis.Value;

    //            var item = valueFactory();
    //            cacheStorage.Put(key, item, config.Ttl);
    //            synchronizer.NotifyUpdate(new[] { key });
    //            return item;
    //        }

    //        var lazyItem = concurrentDictionary.GetOrAdd(key, k => Build(k, syncFac: GetFromOuterFactory));

    //        return lazyItem.ValueEntry.Value;
    //    }

    //    public Task<T> GetOrAddAsync(string key, Func<Task<T>> valueFactory)
    //    {
    //        async Task<T> GetFromOuterFactory()
    //        {
    //            var itemFromRedis = await cacheStorage.GetAsync<T>(key);

    //            if (itemFromRedis.Value != null)
    //                return itemFromRedis.Value;

    //            var item = await valueFactory();
    //            await cacheStorage.PutAsync(key, item, config.Ttl);
    //            await synchronizer.NotifyUpdateAsync(new[] { key });
    //            return item;
    //        }

    //        var lazyItem = concurrentDictionary.GetOrAdd(key, k => Build(k, GetFromOuterFactory));

    //        return lazyItem.TaskEntry.Value;
    //    }

    //    public void Put(string key, T value, TimeSpan? ttl = null)
    //    {
    //        var lazyItem = concurrentDictionary.AddOrUpdate(key, s => Build(key, value), (s, item) => Build(key, value));

    //        cacheStorage.Put(key, lazyItem.ValueEntry.Value, ttl ?? config.Ttl);

    //        synchronizer.NotifyUpdate(new[] { key });
    //    }

    //    public void Put(string key, T value, string region)
    //    {
    //        Put(region + ":" + key, value);
    //    }

    //    public async Task PutAsync(string key, T value)
    //    {
    //        var lazyItem = concurrentDictionary.AddOrUpdate(key, s => Build(s, value), (s, item) => Build(s, value));
    //        await cacheStorage.PutAsync(key, lazyItem.ValueEntry.Value, config.Ttl);
    //        await synchronizer.NotifyUpdateAsync(new[] { key });
    //    }

    //    public async Task PutAllAsync(IEnumerable<KeyValuePair<string, T>> values)
    //    {
    //        await cacheStorage.PutAllAsync(AddToMemory(values), config.Ttl);
    //        await synchronizer.NotifyUpdateAsync(values.Select(x => x.Key).ToArray());
    //    }

    //    private IEnumerable<KeyValuePair<string, T>> AddToMemory(IEnumerable<KeyValuePair<string, T>> values)
    //    {
    //        foreach (var valuePair in values)
    //        {
    //            concurrentDictionary.AddOrUpdate(valuePair.Key, s => Build(valuePair.Key, valuePair.Value), (s, item) => Build(s, valuePair.Value));

    //            yield return valuePair;
    //        }
    //    }

    //    public void PutAll(IEnumerable<KeyValuePair<string, T>> values)
    //    {
    //        cacheStorage.PutAll(AddToMemory(values), config.Ttl);
    //        synchronizer.NotifyUpdate(values.Select(x => x.Key).ToArray());
    //    }

    //    public void Remove(string key)
    //    {
    //        concurrentDictionary.TryRemove(key, out _);
    //        cacheStorage.Remove(key);
    //        synchronizer.NotifyUpdate(new[] { key });
    //    }

    //    public async Task RemoveAsync(string key)
    //    {
    //        concurrentDictionary.TryRemove(key, out _);
    //        await cacheStorage.RemoveAsync(key);
    //        await synchronizer.NotifyUpdateAsync(new[] { key });
    //    }

    //    public void Clear(string pattern)
    //    {
    //        var removedKeys = RemoveFromMemory(pattern);
    //        cacheStorage.Clear(pattern);
    //        synchronizer.NotifyUpdate(removedKeys.ToArray());
    //    }

    //    public async Task ClearAsync(string pattern)
    //    {
    //        if (string.IsNullOrEmpty(pattern))
    //            throw new NullReferenceException(nameof(pattern));

    //        var removedKeys = RemoveFromMemory(pattern);

    //        await cacheStorage.ClearAsync(pattern);
    //        await synchronizer.NotifyUpdateAsync(removedKeys.ToArray());
    //    }

    //    private IEnumerable<string> RemoveFromMemory(string pattern)
    //    {
    //        if (pattern != "*")
    //        {
    //            var expired = new List<string>();
    //            foreach (var item in concurrentDictionary)
    //            {
    //                if (item.Key.IsLike(pattern, true))
    //                    expired.Add(item.Key);
    //            }

    //            foreach (var item in expired)
    //            {
    //                concurrentDictionary.TryRemove(item, out _);
    //            }

    //            return expired;
    //        }
    //        else
    //        {
    //            concurrentDictionary.Clear();
    //            return concurrentDictionary.Keys;
    //        }
    //    }
    //}
}
