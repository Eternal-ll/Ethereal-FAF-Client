using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    internal static class ConfigurationConstants
    {
        public const string ForgedAllianceLocation = "ForgedAlliance:Location";
        public const string ForgedAlliancePatchLocation = "ForgedAlliance:Patch:Location";
        public const string ForgedAllianceVaultLocation = "ForgedAlliance:Vault:Location";


        public const string IceAdapterExecutable = "IceAdapter:Executable";
        public const string IceAdapterForceRelay = "IceAdapter:ForceRelay";
        public const string IceAdapterUseTelemetryUI = "IceAdapter:UseTelemetryUI";
        public const string IceAdapterTelemetry = "IceAdapter:Telemetry";
        public const string IceAdapterTtl = "IceAdapter:Ttl";
    }
    /// <summary>
    /// User config
    /// </summary>
    internal static partial class ConfigurationExtensions
    {
        public static string GetGithubUser(this IConfiguration configuration) =>
            configuration.GetValue<string>("Client:Github:User");
        public static string GetGithubProject(this IConfiguration configuration) =>
            configuration.GetValue<string>("Client:Github:Project");
        public static string GetGithubBranch(this IConfiguration configuration) =>
            configuration.GetValue<string>("Client:Github:Branch");
        public static string GetChangelogUrl(this IConfiguration configuration) =>
            "https://raw.githubusercontent.com/" +
            configuration.GetGithubUser() + '/' +
            configuration.GetGithubProject() + '/' +
            configuration.GetGithubBranch() +
            "/CHANGELOG.md";

        public static string GetUpdateUrl(this IConfiguration configuration) =>
            "https://raw.githubusercontent.com/" +
            configuration.GetGithubUser() + '/' +
            configuration.GetGithubProject() + '/' +
            configuration.GetGithubBranch() +
            "/update.json";


        public static string GetVersion(this IConfiguration configuration) =>
            configuration.GetValue<string>("Client:Version");

        public static bool IsClientUpdated(this IConfiguration configuration) =>
            configuration.GetValue<bool>("Client:IsUpdated", true);

        /// <summary>
        /// Get path to user maps folder
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Path to user maps folder</returns>
        public static string GetMapsLocation(this IConfiguration configuration) =>
            Environment.ExpandEnvironmentVariables(Path.Combine(configuration.GetForgedAllianceVaultLocation(), "maps"));
        /// <summary>
        /// Get full path to subfile of specific map folder
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="mapname">mapname.version</param>
        /// <param name="extension">Example: "_scenario.lua", "_save.lua", "_script.lua", ".scmap"</param>
        /// <param name="version">version</param>
        /// <param name="maps">specific maps folder</param>
        /// <returns>Full path to required file</returns>
        public static string GetMapFile(this IConfiguration configuration, string mapname, string extension,
            string maps = null) =>
        Path.Combine(maps ?? configuration.GetMapsLocation(), mapname, mapname.Split('.')[0] + extension);
        /// <summary>
        /// Get path to user mods folder
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Path to user mods folder</returns>
        public static string GetModsFolder(this IConfiguration configuration) =>
            Environment.ExpandEnvironmentVariables(Path.Combine(configuration.GetForgedAllianceVaultLocation(), "mods"));

        /// <summary>
        /// Get FAF content url
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>FAF content url</returns>
        public static string GetContentUrl(this IConfiguration configuration) =>
            configuration.GetValue<string>("FAForever:Content");

        public static int[] GetPlayersRatingRanges(this IConfiguration configuration) =>
            configuration.GetSection("Client:Players:Ranges").Get<int[]>();

        // javaRuntime: configuration.GetValue<string>("Paths:JavaRuntime"),
        //    logging: configuration.GetValue<string>("MapGenerator:Logs"),
        //    previewPath: configuration.GetValue<string>("MapGenerator:PreviewPath"),
        //    mapGeneratorsFolder: configuration.GetValue<string>("MapGenerator:Versions"),

        #region Map generator settings
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetJavaRuntimeExecutable(this IConfiguration configuration)
            => configuration.GetValue<string>("Paths:JavaRuntime");
        public static string GetMapGeneratorLoggingLocation(this IConfiguration configuration)
            => configuration.GetValue<string>("MapGenerator:Logging");
        public static string GetMapGeneratorPreviewsLocation(this IConfiguration configuration)
            => configuration.GetValue<string>("MapGenerator:PreviewPath");
        public static string GetMapGeneratorVersionsLocation(this IConfiguration configuration)
            => configuration.GetValue<string>("MapGenerator:Versions:Location");
        public static string GetMapGeneratorLatestVersion(this IConfiguration configuration)
            => configuration.GetValue<string>("MapGenerator:Versions:Latest");
        public static Uri GetMapGeneratorGithubRepository(this IConfiguration configuration)
            => configuration.GetValue<Uri>("MapGenerator:Versions:Repository");
        #endregion


        public static string GetForgedAllianceLocation(this IConfiguration configuration) =>
            configuration.GetValue<string>(ConfigurationConstants.ForgedAllianceLocation);
        public static string GetForgedAlliancePatchLocation(this IConfiguration configuration) =>
            configuration.GetValue<string>(ConfigurationConstants.ForgedAlliancePatchLocation);
        public static string GetForgedAllianceVaultLocation(this IConfiguration configuration) =>
            configuration.GetValue<string>(ConfigurationConstants.ForgedAllianceVaultLocation);


        #region Ice adapter

        public static string GetIceAdapterExecutable(this IConfiguration configuration)
            => configuration.GetValue<string>(ConfigurationConstants.IceAdapterExecutable);

        public static bool IceAdapterForceRelay(this IConfiguration configuration)
            => configuration.GetValue<bool>(ConfigurationConstants.IceAdapterForceRelay, false);

        public static bool IceAdapterUseTelemetryUI(this IConfiguration configuration)
            => configuration.GetValue<bool>(ConfigurationConstants.IceAdapterUseTelemetryUI, false);
        public static string GetIceAdapterTelemtryUrl(this IConfiguration configuration)
            => configuration.GetValue<string>(ConfigurationConstants.IceAdapterTelemetry);

        public static bool IceAdapterHasWebUiSupport(this IConfiguration configuration)
            => configuration.GetIceAdapterExecutable().Contains("3.3");

        public static int GetIceAdapterTtl(this IConfiguration configuration)
            => configuration.GetValue<int>(ConfigurationConstants.IceAdapterTtl, 86400);
        #endregion
    }
}
