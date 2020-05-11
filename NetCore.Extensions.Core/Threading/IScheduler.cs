using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCore.Extensions.Core.Threading
{
    public interface IScheduler : IDisposable
    {
        IDisposable Schedule(Action<object> action, object state, TimeSpan dueTime, TimeSpan period);
    }

    public class Scheduler : IScheduler
    {
        class TimerScheduler : IDisposable
        {
            private readonly Scheduler scheduler;
            private readonly System.Threading.Timer internalTimer;

            public TimerScheduler(Action<object> action, object state, TimeSpan dueTime, TimeSpan period, Scheduler scheduler)
            {
                this.scheduler = scheduler;
                internalTimer = new System.Threading.Timer(obj =>
                {
                    try
                    {
                        action(obj);
                    }
                    catch 
                    {
                    }
                }, state, dueTime, period);
            }

            public void Dispose()
            {
                internalTimer.Dispose();
                scheduler.RemoveScheduler(this);
            }
        }

        readonly HashSet<TimerScheduler> schedulers = new HashSet<TimerScheduler>();

        readonly object sync = new object();

        public Scheduler()
        {
            if (Instance == null)
                Instance = this;
        }

        public static IScheduler Instance { get; private set; }

        private void AddScheduler(TimerScheduler scheduler)
        {
            lock (sync)
            {
                schedulers.Add(scheduler); 
            }
        }

        private void RemoveScheduler(TimerScheduler scheduler)
        {
            lock (sync)
            {
                schedulers.Add(scheduler);
            }
        }

        public IDisposable Schedule(Action<object> action, object state, TimeSpan dueTime, TimeSpan period)
        {
            var scheduler = new TimerScheduler(action, state, dueTime, period, this);
            AddScheduler(scheduler);
            return scheduler;
        }

        public void Dispose()
        {
            foreach (var scheduler in schedulers.ToArray())
            {
                scheduler.Dispose();
            }
        }
    }
}
