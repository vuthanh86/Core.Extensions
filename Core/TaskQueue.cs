using System;
using System.Threading.Tasks;

namespace Core
{
    public class TaskQueue<TIngredient, TData>
    {
        private readonly Func<TData, Task> consumerAction;
        private readonly Func<TIngredient, Task<TData>> transformer;

        public TaskQueue(Func<TData, Task> consumerAction, Func<TIngredient, Task<TData>> transformer)
        {
            this.consumerAction = consumerAction;
            this.transformer = transformer;
        }

        public void Post(TIngredient item)
        {
            TaskPool.Queue(async () =>
            {
                try
                {
                    var t = await transformer(item);
                    await consumerAction(t);
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(item, ex);
                }
            });
        }

        public event Action<TIngredient, Exception> OnException;
    }

    public class TaskQueue<TData>: TaskQueue<TData, TData>
    {
        public TaskQueue(Func<TData, Task> consumerAction) : base(consumerAction, Task.FromResult)
        {
        }
    }

    public static class TaskPool
    {
        public static void Queue(Func<Task> task)
        {
            Task.Factory.StartNew(task);
        }

        //private static readonly TaskQueue<Func<Task>> TaskQueues = new TaskQueue<Func<Task>>(Consumer);

        //private static async Task Consumer(Func<Task> arg)
        //{
        //    await arg();
        //}

        //public static void Queue(Func<Task> task)
        //{
        //    TaskQueues.Post(task);
        //}
    }
}