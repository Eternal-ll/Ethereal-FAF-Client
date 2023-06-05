using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Queue
{
    interface IQueueService
    {
        public Task Initialize(CancellationToken cancellationToken = default);
        public Task<bool> GetQueueId(string queue);
    }
}
