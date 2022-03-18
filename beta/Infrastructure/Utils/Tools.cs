using System;
using System.IO;
using System.Net;
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

        public static long GetFileSize(string url)
        {
            long result = -1;
            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            using (WebResponse resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                {
                    result = ContentLength;
                }
            }
            return result;
        }
        public static long GetFileSize(Uri uri) => GetFileSize(uri.OriginalString);
    }
}
