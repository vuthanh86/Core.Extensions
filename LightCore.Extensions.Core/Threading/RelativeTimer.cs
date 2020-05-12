using System;
using System.Threading.Tasks;

namespace NetCore.Extensions.Core.Threading
{
    public class RelativeTimer:IDisposable
    {
        private readonly Func<object, Task> _action;
        private readonly object _state;
        private readonly int _period;
        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="delay">delay in miliseconds</param>
        /// <param name="period">period in miliseconds</param>
        public RelativeTimer(Func<object,Task> action, object state, int delay, int period)
        {
            _action = action;

            if(delay <0)
                throw new ArgumentException("delay should >= 0");
            
            if(period <1)
                throw new ArgumentException("period should > 1");

            _state = state;
            _period = period;

            Queue(delay);
        }

        private void Queue()
        {
            Queue(_period);
        }

        private void Queue(int delay)
        {
            if (_disposed) return;

            TaskPool.Queue(async () =>
            {
                await Task.Delay(delay);
                await _action(_state)
                    .ContinueWith(task =>
                    {
                        if (_disposed) return;
                        Queue();
                    });
            });
        }

        #region IDisposable

        public void Dispose()
        {
            _disposed = true;
        }

        #endregion
    }
}
