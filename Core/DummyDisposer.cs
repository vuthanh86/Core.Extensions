using System;

namespace Core
{
    public class DummyDisposer : IDisposable
    {
        private readonly Action onDisposed;

        public DummyDisposer(Action onDisposed)
        {
            this.onDisposed = onDisposed;
        }

        public void Dispose()
        {
            onDisposed?.Invoke();
        }
    }
}
