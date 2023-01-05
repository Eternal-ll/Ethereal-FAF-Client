using System.Text;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    internal static class IceAdapterArguments
    {
        public static StringBuilder Generate(string iceJarFile) => new($"-jar \"{iceJarFile}\" ");
        public static StringBuilder WithPlayerId(this StringBuilder sb, long id)
            => sb.Append($"--id {id} ");
        public static StringBuilder WithPlayerLogin(this StringBuilder sb, string login)
            => sb.Append($"--login {login} ");
        public static StringBuilder WithGameId(this StringBuilder sb, long id, bool include = false)
            => include ? sb.Append($"--game-id {id} ") : sb;
        public static StringBuilder WithRpcPort(this StringBuilder sb, int port)
            => sb.Append($"--rpc-port {port} ");
        public static StringBuilder WithGPGNetPort(this StringBuilder sb, int port)
            => sb.Append($"--gpgnet-port {port} ");
        public static StringBuilder WithForcedRelay(this StringBuilder sb, bool force = false)
            => force ? sb.Append("--force-relay ") : sb;
        public static StringBuilder WithWebUi(this StringBuilder sb, long gameId, bool enabled = false)
            => enabled ? sb.Append($"--game-id {gameId} ") : sb;
    }
}
