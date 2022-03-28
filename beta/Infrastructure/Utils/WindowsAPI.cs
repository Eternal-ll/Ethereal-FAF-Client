using System;
using System.Runtime.InteropServices;
using System.Text;

namespace beta.Infrastructure.Utils
{
    public static class WindowsAPI
    {
        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);
        
        // Return a file size created by the StrFormatByteSize API function.
        public static string ToFileSize(this long file_size)
        {
            StringBuilder sb = new (20);
            StrFormatByteSize(file_size, sb, 20);
            return sb.ToString();
        }
    }
}
