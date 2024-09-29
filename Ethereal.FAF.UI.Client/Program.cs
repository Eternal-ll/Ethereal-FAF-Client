using CommandLine;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Updater;
using Ethereal.FAF.UI.Client.Models;
using Polly.Contrib.WaitAndRetry;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client
{
    public static class Program
    {
        private static ILogger _logger;
        private static ILogger Logger => _logger ??= Serilog.Log.Logger;
        public static AppArgs Args { get; private set; } = new();
        [STAThread]
        public static void Main(string[] args)
        {
            // force ipv4 for a while
            AppContext.SetSwitch("System.Net.DisableIPv6", true);
            var parseResult = Parser
                .Default
                .ParseArguments<AppArgs>(args)
                .WithNotParsed(errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error.ToString());
                    }
                });

            if (Args.WaitForExitPid is { } waitExitPid)
            {
                WaitForPidExit(waitExitPid, TimeSpan.FromSeconds(30));
            }

            HandleUpdateReplacement();
            HandleUpdateCleanup();

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                Logger.Error("Unobserved task error");
                Logger.Error(ex.ToString());
            }
        }

        /// <summary>
        /// Wait for an external process to exit,
        /// ignores if process is not found, already exited, or throws an exception
        /// </summary>
        /// <param name="pid">External process PID</param>
        /// <param name="timeout">Timeout to wait for process to exit</param>
        private static void WaitForPidExit(int pid, TimeSpan timeout)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                process.WaitForExitAsync(new CancellationTokenSource(timeout).Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                Logger.Warning("Timed out ({Timeout:g}) waiting for process {Pid} to exit", timeout, pid);
            }
            catch (SystemException e)
            {
                Logger.Warning(e, "Failed to wait for process {Pid} to exit", pid);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected error during WaitForPidExit");
                throw;
            }
        }
        private static void HandleUpdateReplacement()
        {
            var currentDir = AppHelper.ExecutableDirectory;

            // Check if we're in the named update folder or the legacy update folder for 1.2.0 -> 2.0.0
            if (currentDir.Name is not UpdateHelper.UpdateFolderName)
                return;

            if (currentDir.Parent is not { } parentDir)
                return;

            // Copy our current file to the parent directory, overwriting the old app file
            var currentExe = new FileInfo(AppHelper.ExecutableFileName);
            var targetExe = Path.Combine(parentDir.FullName, AppHelper.GetExecutableName());

            var isCopied = false;

            foreach (
                var delay in Backoff.DecorrelatedJitterBackoffV2(
                    TimeSpan.FromMilliseconds(300),
                    retryCount: 6,
                    fastFirst: true
                )
            )
            {
                try
                {
                    currentExe.CopyTo(targetExe, true);
                    isCopied = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(delay);
                }
            }

            if (!isCopied)
            {
                Logger.Error("Failed to copy current executable to parent directory");
                Environment.Exit(1);
            }

            // Start the new app while passing our own PID to wait for exit
            Process.Start(targetExe, $"--wait-for-exit-pid {Environment.ProcessId}");

            // Shutdown the current app
            Environment.Exit(0);
        }
        private static void HandleUpdateCleanup()
        {
            // Delete update folder if it exists in current directory
            if (UpdateHelper.UpdateFolder is { Exists: true } updateDir)
            {
                try
                {
                    updateDir.Delete(true);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to delete update folder");
                }
            }
        }
    }
}
