using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Collections;
using Microsoft.EntityFrameworkCore;

namespace Core.Repository
{
    public interface IEntityQueue
    {
        Task Insert<T>(T entity) where T: class ;
    }

    public class EntityQueue : IEntityQueue 
    {
        private readonly IDbContextFactory contextFactory;
        private readonly LazyDictionary<Type, Queue> xqueues = new LazyDictionary<Type, Queue>();

        private bool disposed;
        private bool writing;
        private readonly Timer timer;

        public EntityQueue(IDbContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
            timer = new Timer(Loop, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2));
        }

        private DbContext GetContext()
        {
            return contextFactory.CreateDbContext();
        }

        public Task Insert<T>(T entity) where T : class
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Unable to access disposed EntityQueue");
            }

            var record = new Record
            {
                Entity = entity,
                TaskSource = new TaskCompletionSource<bool>()
            };

            xqueues.GetOrAdd(typeof (T), t =>
            {
                return new Queue((ctx, items) =>
                {
                    ctx.Set<T>().AddRange(items.Cast<T>());
                });
            }).Enqueue(record);

            return record.TaskSource.Task;
        }

        private void CancelRemainInQueue()
        {
            var exception = new ObjectDisposedException("Queue was disposed");

            foreach (var entry in xqueues)
            {
                Record r;
                var queue = entry.Value;
                while (queue.TryDequeue(out r))
                {
                    r.TaskSource.SetException(exception);
                }
            }
        }

        private IEnumerable<Record> DeQueueAll(Queue queue)
        {
            var n = queue.Count;
            Record item;
            while (n>0 && queue.TryDequeue(out item))
            {
                yield return item;
                n--;
            }
        }

        private void Loop(object state)
        {
            if (!disposed)
            {
                writing = true;

                foreach (var entry in xqueues)
                {
                    var records = DeQueueAll(entry.Value).ToArray();

                    var offset = 0;
                    while (offset < records.Length)
                    {
                        //                        using (var scope = new TransactionScope())
                        {
                            Record[] availables;
                            Exception exception = null;
                            using (var context = GetContext())
                            {
                                availables = records.Skip(offset).Take(1000).ToArray();

                                offset += availables.Length;

                                entry.Value.Process(context, availables.Select(r=>r.Entity));

                                try
                                {
                                    context.SaveChanges();
                                }
                                catch (Exception e)
                                {
                                    exception = e;
                                }
                            }
                            if (exception != null)
                            {
                                foreach (var record in availables)
                                {
                                    record.TaskSource.SetException(exception);
                                }
                                continue;
                            }

                            foreach (var record in availables)
                            {
                                record.TaskSource.SetResult(true);
                            }

                            //                            scope.Complete();
                        }
                    }
                    writing = false;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            timer.Dispose();

            if (disposing)
            {
                disposed = true;
                for (var i = 0; i < 100; i++) //WTF?
                {
                    if (writing)
                    {
                        Thread.Sleep(10);
                    }
                }
                // if (writing)
                // {
                //    loopThread.Abort();
                // }
                CancelRemainInQueue();
            }

            disposed = true;
        }

        #endregion IDisposable Members

        #region Nested Types

        private class Record
        {
            public object Entity { get; set; }
            public TaskCompletionSource<bool> TaskSource { get; set; }
        }

        private class Queue : ConcurrentQueue<Record>
        {
            private readonly Action<DbContext, IEnumerable<object>> handler;

            public Queue(Action<DbContext, IEnumerable<object>> handler)
            {
                this.handler = handler;
            }

            internal void Process(DbContext context, IEnumerable<object> entities)
            {
                handler.Invoke(context,entities);
            }
        }

        #endregion Nested Types
    }
}
