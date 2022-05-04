using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Extensions
{
    public static class TaskExtensions
    {
        public static Task<T> AsCancellable<T>(this Task<T> task, CancellationToken token)
        {
            if (!token.CanBeCanceled)
            {
                return task;
            }

            var tcs = new TaskCompletionSource<T>();
            // This cancels the returned task:
            // 1. If the token has been canceled, it cancels the TCS straightaway
            // 2. Otherwise, it attempts to cancel the TCS whenever
            //    the token indicates cancelled
            token.Register(() => tcs.TrySetCanceled(token),
                useSynchronizationContext: false);

            task.ContinueWith(t =>
            {
                // Complete the TCS per task status
                // If the TCS has been cancelled, this continuation does nothing
                if (task.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else if (task.IsFaulted)
                {
                    tcs.TrySetException(t.Exception);
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            return tcs.Task;
        }
    }
}
