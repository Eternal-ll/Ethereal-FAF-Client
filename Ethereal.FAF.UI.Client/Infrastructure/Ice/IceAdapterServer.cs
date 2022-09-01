using System.Diagnostics;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IceServer
    {
        public readonly Process Process;
        public IceServer(string playerId, string playerLogin, int rpcPort, int gpgnetPort, string file, string java)
        {
            // load settings to show ice window
            // ...
            // delay window
            // ...
            var args = $"--id {playerId} --login {playerLogin} --rpc-port {rpcPort} --gpgnet-port {gpgnetPort} --log-level debug";
            // --info-window
            // --delay-ui time
            Process = new()
            {
                StartInfo = new()
                {
                    FileName = java,
                    Arguments = $"-jar \"{file}\" {args}",
                    UseShellExecute = false,
                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    //CreateNoWindow = true,
                }
            };
            Process.Start();
        }
    }
}
