using System.Security.Cryptography;
using System.Text;

namespace beta.Infrastructure.Utils
{
    public static class Tools
    {
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
            _ => pixel
            //_ => throw new NotImplementedException()
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
    }
}
