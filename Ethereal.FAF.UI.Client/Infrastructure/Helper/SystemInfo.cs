using Serilog;
using System;
using System.IO;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{

    public static class SystemInfo
    {
        public const long Gibibyte = 1024 * 1024 * 1024;
        public const long Mebibyte = 1024 * 1024;

        public static long? GetDiskFreeSpaceBytes(string path)
        {
            try
            {
                var drive = new DriveInfo(path);
                return drive.AvailableFreeSpace;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }

            return null;
        }
    }
}
