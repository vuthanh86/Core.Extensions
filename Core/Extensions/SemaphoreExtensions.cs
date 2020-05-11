using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class SemaphoreExtensions
    {
        public static async Task<IDisposable> Acquire(this SemaphoreSlim semaphore, TimeSpan?timeout = null)
        {
            if (timeout.HasValue)
                await semaphore.WaitAsync(timeout.Value);
            else
                await semaphore.WaitAsync();

            return new DummyDisposer(() =>
            {
                semaphore.Release();
            });
        }
    }
}
