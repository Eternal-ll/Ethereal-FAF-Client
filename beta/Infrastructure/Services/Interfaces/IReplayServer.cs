using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    internal interface IReplayServerService
    {
        public Task Start();
        public Task Restart();
        public Task Close();
    }
}
