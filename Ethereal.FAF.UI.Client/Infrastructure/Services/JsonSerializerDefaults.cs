using FAF.Domain.LobbyServer.Converters;
using System.Text.Json;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal static class JsonSerializerDefaults
    {
        static JsonSerializerDefaults()
        {
            FafLobbyCommandsJsonSerializerOptions.Converters.Add(new LobbyMessageJsonConverter());
        }
        public static JsonSerializerOptions FafLobbyCommandsJsonSerializerOptions = new();
    }
}
