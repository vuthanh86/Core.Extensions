using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Tracking;

namespace Core.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> CastTo<T>(this Task<object> task)
        {
            return (T) await task;
        }

        public static async Task<object> Boxing<T>(this Task<T> task)
        {
            return await task;
        }

        public static async Task<T> UnBoxing<T>(this Task<object> task)
        {
            var obj = await task;
            
            if (obj == null) return default(T);

            return (T) obj;
        }

        public static async Task<T> Continue<T>(this Task task, Func<Task<T>> continueFunc)
        {
            await task;
            return await continueFunc();
        }

        public static async Task<T> Continue<T>(this Task task, Func<T> continueFunc)
        {
            await task;
            return continueFunc();
        }

        public static async Task<TNewResult> Continue<TOldResult, TNewResult>(this Task<TOldResult> task, Func<TOldResult, TNewResult> continueFunc)
        {
            return continueFunc(await task);
        }

        public static async Task ContinueAsync(this Task task, Func<Task> continueFunc)
        {
            await task;
            await continueFunc();
        }

        public static async Task<TResult> ContinueAsync<TResult>(this Task task, Func<Task<TResult>> continueFunc)
        {
            await task;
            return await continueFunc();
        }

        public static async Task Continue<TOldResult>(this Task<TOldResult> task, Action<TOldResult> continueFunc)
        {
            continueFunc(await task);
        }

        public static async Task<TNewResult> ContinueAsync<TOldResult, TNewResult>(this Task<TOldResult> task, Func<TOldResult, Task<TNewResult>> continueFunc)
        {
            return await continueFunc(await task);
        }

        public static async Task ContinueAsync(this IEnumerable<Task> tasks, Func<Task> continueFunc)
        {
            await Task.WhenAll(tasks);
            await continueFunc();
        }

        public static async Task<TResult> ContinueAsync<TResult>(this IEnumerable<Task> tasks, Func<Task<TResult>> continueFunc)
        {
            await Task.WhenAll(tasks);
            return await continueFunc();
        }

        public static async Task ContinueAsync<TOldResult>(this Task<TOldResult> task, Func<TOldResult, Task> continueFunc)
        {
            await continueFunc(await task);
        }

        public static async Task WithTimeoutCancellation(this Task task, TimeSpan ts)
        {
            using (var ct = new CancellationTokenSource(ts))
            {
                await task.WithCancellation(ct.Token);
            }
        }

        public static async Task<T> WithTimeoutCancellation<T>(this Task<T> task, TimeSpan ts)
        {
            using (var ct = new CancellationTokenSource(ts))
            {
                await task.WithCancellation(ct.Token);
                return task.Result;
            }
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var cancellationCompletionSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    throw new TaskCanceledException();
                }
            }

            await task;
        }

        public static void Queue(this Func<Task> func)
        {
            TaskPool.Queue(func);
        }

        public static void Queue(this Task task)
        {
            TaskPool.Queue(()=>task);
        }


        public static async Task Track(this Task task, string eventName, params string[] tags)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                try
                {
                    await TrackingAgent.Measure(task, eventName, tags);
                }
                catch (Exception e)
                {
                    TrackingAgent.Error(eventName, e);
                    throw;
                }
            }
            else
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    TrackingAgent.Error(eventName, task.Exception, tags);
                }
            }
        }

        public static async Task<T> Track<T>(this Task<T> task, string eventName, params string[] tags)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                try
                {
                    return await TrackingAgent.Measure(task, eventName, tags);
                }
                catch (Exception e)
                {
                    TrackingAgent.Error(eventName, e, tags);
                    throw;
                }
            }
            else
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    TrackingAgent.Error(eventName, task.Exception, tags);
                }
            }
            return task.Result;
        }
    }
}