using CommandLine;

namespace Ethereal.FAF.UI.Client.Models
{
    public class AppArgs
    {
        /// <summary>
        /// If provided, the app will wait for the process with this PID to exit
        /// before starting up. Mainly used by the updater.
        /// </summary>
        [Option("wait-for-exit-pid", Hidden = true)]
        public int? WaitForExitPid { get; set; }
    }
}
