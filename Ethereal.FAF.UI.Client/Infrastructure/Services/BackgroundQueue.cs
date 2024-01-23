using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public abstract class BackgroundQueue : IBackgroundQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
        private SemaphoreSlim _signal = new(0);
        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            _workItems.Enqueue(workItem);
            _signal.Release();
        }
        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem;
        }
    }
}
