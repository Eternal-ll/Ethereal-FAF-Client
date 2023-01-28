using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// System UID generator using lobby session code
    /// </summary>
    public sealed class UidGenerator
    {
        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;
        public UidGenerator(ILogger<UidGenerator> logger, IConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        public async Task<string> GenerateAsync(string session)
        {
            var executable = Configuration.GetUidGeneratorExecutable();
            if (!System.IO.File.Exists(executable))
            {
                throw new System.IO.FileNotFoundException("Can`t find executable of UID generator", executable);
            }
            Logger.LogTrace("Generating UID for session [{session}]", session);
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = executable,
                    Arguments = session,
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
            Logger.LogTrace("Closing UID generator...");
            process.Close();
            process.Dispose();
            Logger.LogTrace("UID closed");
            return result;
        }
    }
}
