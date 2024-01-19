using System;
using System.Reflection;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    internal static class VersionHelper
    {
        public static Version GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return new Version(version.Major, version.Minor, version.Build);
        }
        public static string GetCurrentVersionInText() => GetCurrentVersion().ToString();
    }
}
