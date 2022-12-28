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
        public static string GetMapsFolder(this IConfiguration configuration) =>
            Environment.ExpandEnvironmentVariables(Path.Combine(configuration.GetValue<string>("Paths:Vault"), "maps"));
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
        Path.Combine(maps ?? configuration.GetMapsFolder(), mapname, mapname.Split('.')[0] + extension);
        /// <summary>
        /// Get path to user mods folder
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>Path to user mods folder</returns>
        public static string GetModsFolder(this IConfiguration configuration) =>
            Environment.ExpandEnvironmentVariables(Path.Combine(configuration.GetValue<string>("Paths:Vault"), "mods"));

        /// <summary>
        /// Get FAF content url
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>FAF content url</returns>
        public static string GetContentUrl(this IConfiguration configuration) =>
            configuration.GetValue<string>("FAForever:Content");

        public static int[] GetPlayersRatingRanges(this IConfiguration configuration) =>
            configuration.GetSection("Client:Players:Ranges").Get<int[]>();



        public static string GetForgedAllianceLocation(this IConfiguration configuration) =>
            configuration.GetValue<string>(ConfigurationConstants.ForgedAllianceLocation);
        public static string GetForgedAlliancePatchLocation(this IConfiguration configuration) =>
            configuration.GetValue<string>(ConfigurationConstants.ForgedAlliancePatchLocation);
        public static string GetForgedAllianceVaultLocation(this IConfiguration configuration) =>
            configuration.GetValue<string>(ConfigurationConstants.ForgedAllianceVaultLocation);
    }
}
