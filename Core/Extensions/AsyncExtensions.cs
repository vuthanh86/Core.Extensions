using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class AsyncExtension
    {
        private static readonly TaskFactory TaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void RunSync(Func<Task> func)
        {
            TaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        public static async Task<bool> TrueForAllAsync(this IEnumerable<Task<bool>> tasks)
        {
            return await tasks.TrueForAllAsync(t => t);
        }

        public static async Task<bool> TrueForAnyAsync(this IEnumerable<Task<bool>> tasks)
        {
            return await tasks.TrueForAnyAsync(t => t);
        }

        public static async Task<bool> TrueForAllAsync<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
        {
            foreach (var task in tasks)
            {
                var t = await task;
                if (!predicate(t))
                    return false;
            }
            return true;
        }

        public static async Task<bool> TrueForAnyAsync<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
        {
            foreach (var task in tasks)
            {
                var t = await task;
                if (predicate(t))
                    return true;
            }
            return false;
        }
    }
}
