using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore.Extensions.Entity
{
    public class MemCacheService<TKey,TValue>
    {
         private readonly TimeSpan _expiry;
        private readonly TimeSpan _factoryTimeout;
        private readonly TimeSpan _intervalCheck;

        private readonly Func<IReadOnlyCollection<TKey>,CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> factory;

        private readonly ConcurrentDictionary<TKey, Record> dictionary = new ConcurrentDictionary<TKey, Record>();
        private readonly ConcurrentQueue<Record> queue = new ConcurrentQueue<Record>();

        private class Record
        {
            public readonly DateTime ExpiredAt;
            private readonly TaskCompletionSource<TValue> _taskCompletionSource;

            public Record(TKey key,DateTime expiredAt)
            {
                Key = key;
                ExpiredAt = expiredAt;
                _taskCompletionSource =new TaskCompletionSource<TValue>();
            }

            public Task<TValue> ValueTask => _taskCompletionSource.Task;
            public TKey Key { get; }

            public void SetValue(TValue value)
            {
                _taskCompletionSource.SetResult(value);
            }
        }

        public MemCacheService(TimeSpan expiry, TimeSpan expiryIntervalCheck, TimeSpan factoryTimeout,
            Func<IReadOnlyCollection<TKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> factory)
        {
            _intervalCheck = expiryIntervalCheck;
            this._expiry = expiry;
            this._factoryTimeout = factoryTimeout;
            this.factory = factory;
            Task.Factory.StartNew(CleanUp);
        }

        private async Task CleanUp()
        {
            try
            {
                var n = DateTime.Now;

                while (queue.TryPeek(out var onTop) && onTop.ExpiredAt < n)
                {
                    dictionary.TryRemove(onTop.Key, out _);
                    queue.TryDequeue(out _);
                }
            }
            finally
            {
                await Task.Delay(_intervalCheck).ConfigureAwait(false);
                await Task.Factory.StartNew(CleanUp).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// get cache async. If cache is not exist and factory unable to produce cache entry Cache value will be null, need to check KeyValuePair.Value null
        /// </summary>
        public async Task<IEnumerable<KeyValuePair<TKey, TValue>>> GetAsync(IEnumerable<TKey> keys)
        {
            var missing = new List<Record>();
            var results = new List<Record>();
            
            var n = DateTime.Now;
            var newRecordExpiredAt = n.Add(_expiry);

            foreach (var k in keys)
            {
                var record = dictionary.GetOrAdd(k, key =>
                {
                    var r = new Record(key, newRecordExpiredAt);
                    queue.Enqueue(r);
                    missing.Add(r);
                    return r;
                });

                results.Add(record);
            }

            Request(missing);

            await Task.WhenAll(results.Select(x => x.ValueTask).ToList());
            return results.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.ValueTask.Result));
        }

        private void Request(IReadOnlyCollection<Record> records)
        {
            Task.Factory.StartNew(async () =>
            {
                using (var cts = new CancellationTokenSource(_factoryTimeout))
                {
                    try
                    {
                        var result = (await factory(records.Select(x => x.Key).ToList(), cts.Token)).ToDictionary(x => x.Key, x => x.Value);

                        foreach (var r in records)
                        {
                            if (result.TryGetValue(r.Key, out var value))
                            {
                                r.SetValue(value);
                            }
                            else
                            {
                                r.SetValue(default(TValue));
                            }
                        }
                    }
                    catch(Exception)
                    {
                        foreach (var r in records)
                        {
                            r.SetValue(default(TValue));
                        }
                    }
                }
            });
        }
    }
}