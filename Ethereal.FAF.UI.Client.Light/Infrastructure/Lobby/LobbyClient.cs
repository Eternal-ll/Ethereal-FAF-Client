using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Light.Infrastructure.Lobby
{
    public class LobbyClient : PipeTcpClient
    {
        public LobbyClient(ILogger<LobbyClient> logger) : base(logger)
        {

        }

        public override Task HandleMessage(string message)
        {
            Logger.LogInformation(message);
            return Task.CompletedTask;
        }
    }
}
