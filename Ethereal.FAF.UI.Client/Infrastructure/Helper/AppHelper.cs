using System;
using System.Diagnostics;
using System.IO;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    internal static class AppHelper
    {
        public static DirectoryInfo FilesDirectory { get; }
        public static DirectoryInfo ExecutableDirectory { get; }
        public static string ExecutableFileName => Path.Combine(ExecutableDirectory.FullName, GetExecutableName());
        /// <summary>
        /// Get the current application executable name.
        /// </summary>
        public static string GetExecutableName()
        {
            using var process = Process.GetCurrentProcess();
            var fullPath = process.MainModule?.ModuleName;
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new Exception("Could not find executable name");
            }
            return Path.GetFileName(fullPath);
        }
        static AppHelper()
        {
            using var process = Process.GetCurrentProcess();
            ExecutableDirectory = new(Path.GetDirectoryName(process.MainModule.FileName));
            FilesDirectory = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files"));
            if (!FilesDirectory.Exists) FilesDirectory.Create();
        }
    }
}
