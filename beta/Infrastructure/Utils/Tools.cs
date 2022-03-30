using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace beta.Infrastructure.Utils
{
    public static class Tools
    {
        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        public static DependencyObject FindChild(DependencyObject parent, string name)
        {
            // confirm parent and name are valid.
            if (parent is null || string.IsNullOrEmpty(name)) return null;


            if ((parent as FrameworkElement)?.Name == name) return parent;

            DependencyObject result = null;

            (parent as FrameworkElement)?.ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                result = FindChild(child, name);
                if (result is not null) break;
            }

            return result;
        }

        /// <summary>
        /// Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // confirm parent is valid.
            if (parent is null) return null;
            if (parent is T) return parent as T;

            DependencyObject foundChild = null;

            (parent as FrameworkElement)?.ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foundChild = FindChild<T>(child);
                if (foundChild is not null) break;
            }

            return foundChild as T;
        }


        public static string CalculateMD5FromText(string text)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        public static string CalculateMD5FromFile(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static int CalculateMapSizeToKm(int pixel) => pixel switch
        {
            64 => 1,
            128 => 2,
            256 => 5,
            512 => 10,
            1024 => 20,
            2048 => 40,
            4096 => 80,
            _ => throw new NotImplementedException()
        };
        public static int CalculateMapSizeToPixels(int km) => km switch
        {
            1 => 64,
            2 => 128,
            5 => 256,
            10 => 512,
            20 => 1024,
            40 => 2048,
            80 => 4096,
            _ => throw new NotImplementedException()
        };

        public static string CalcMemoryMensurableUnit(this long bytes) => CalcMemoryMensurableUnit((double)bytes);

        public static string CalcMemoryMensurableUnit(this double bytes)
        {
            double kb = bytes / 1024; // · 1024 Bytes = 1 Kilobyte 
            double mb = kb / 1024; // · 1024 Kilobytes = 1 Megabyte 
            double gb = mb / 1024; // · 1024 Megabytes = 1 Gigabyte 
            double tb = gb / 1024; // · 1024 Gigabytes = 1 Terabyte 

            string result =
                tb > 1 ? $"{tb:0.##}TB" :
                gb > 1 ? $"{gb:0.##}GB" :
                mb > 1 ? $"{mb:0.##}MB" :
                kb > 1 ? $"{kb:0.##}KB" :
                $"{bytes:0.##}B";

            result = result.Replace("/", ".");
            return result;
        }

        public static FileInfo GetFafUidFileInfo() => new(ExtractAndReturnPath(Properties.Resources.faf_uid, "faf-uid.exe"));
        public static FileInfo GetIceAdapterJarFileInfo() => new(ExtractAndReturnPath(Properties.Resources.faf_ice_adapter, "faf-ice-adapter"));
        private static string ExtractAndReturnPath(byte[] binary, string name)
        {
            string path = App.CurrentDirectory + "\\Third-party applications\\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += name;
            if (File.Exists(path)) return path;
            using FileStream fs = new(path, FileMode.Create);
            fs.Write(binary);
            return path;
        }
    }
}
