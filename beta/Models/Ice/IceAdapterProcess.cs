using beta.Infrastructure.Utils;
using beta.Models.Debugger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace beta.Models.Ice
{
    internal class IceAdapterProcess
    {
        public Process Process { get; private set; }
        private readonly ILogger Logger = App.Services.GetService<ILogger<IceAdapterProcess>>();
        public IceAdapterProcess(string playerId, string playerLogin, int RpcPort, int GpgNetPort)
        {
            // load settings to show ice window
            // ...
            // delay window
            // ...
            var file = Tools.GetIceAdapterJarFileInfo();
            var args = $"--id {playerId} --login {playerLogin} --rpc-port {RpcPort} --gpgnet-port {GpgNetPort} --log-level debug";
            // --info-window
            // --delay-ui time
            Process = new()
            {
                StartInfo = new()
                {
                    FileName = $"java",
                    Arguments = $"-jar \"{file.FullName}\" {args}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    //CreateNoWindow = true,
                }
            };
            Logger.LogInformation($"Initializing {file.FullName} with {args}");
            try
            {
                Process.Start();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            //Process.StartInfo.Environment.Add("LOG_DIR", @"C:\Program Files (x86)\Forged Alliance Forever\lib\ice-adapter\logs\");
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
#if DEBUG
            Process.OutputDataReceived += OnIceOutputDataReceived;
            Process.ErrorDataReceived += OnIceErrorDataReceived;
            Process.Exited += OnIceExited;
#endif
            Logger.LogInformation("Local ice server initialized");
        }

        private void OnIceExited(object sender, System.EventArgs e)
        {
            AppDebugger.LOGICE($"{Process.ProcessName} exited");
        }

        private void OnIceOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //AppDebugger.LOGICE(e.Data);
        }

        private void OnIceErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AppDebugger.LOGICE(e.Data);
        }
    }
}
