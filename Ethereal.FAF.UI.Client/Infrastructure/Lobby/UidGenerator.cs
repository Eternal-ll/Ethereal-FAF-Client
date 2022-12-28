using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public sealed class UidGenerator
    {
        private readonly ILogger Logger;
        private readonly string Uid;
        public UidGenerator(ILogger logger, string uid)
        {
            Logger = logger;
            Uid = uid;
        }

        public async Task<string> GenerateUID(string session, IProgress<string> progress = null)
        { 
            Logger.LogTrace("Generating UID for session [{session}]", session);
            progress?.Report($"Generating UID for session: {session}");
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = Uid,
                    Arguments = session,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            Logger.LogTrace("Launching UID generator on [{fafuid}]", process.StartInfo.FileName);
            process.Start();
            Logger.LogTrace("Reading output...");
            string result = await process.StandardOutput.ReadLineAsync();
            Logger.LogTrace("Done reading ouput");
            Logger.LogTrace("Generated UID: [**********]");
            progress?.Report($"Generated UID: {result[..10]}...");
            Logger.LogTrace("Closing UID generator...");
            process.Close();
            process.Dispose();
            Logger.LogTrace("UID closed");
            return result;
        }
    }
}
