using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Utils
{
    internal static class ImageTools
    {
        public static BitmapImage InitializeLazyBitmapImage(string url, int? decodeWidth = null, int? decodeHeight = null) =>
            InitializeLazyBitmapImage(new Uri(url), decodeWidth, decodeHeight);
        public static BitmapImage InitializeLazyBitmapImage(Uri url, int? decodeWidth = null, int? decodeHeight = null)
        {
            var dispatcher = App.Current.Dispatcher;
            return dispatcher.CheckAccess()
                ? GetImage(url, decodeWidth, decodeHeight)
                : dispatcher.Invoke(() => GetImage(url, decodeWidth, decodeHeight),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private static BitmapImage GetImage(Uri url, int? decodeWidth = null, int? decodeHeight = null)
        {
            var img = new BitmapImage();
            img.BeginInit();
            if (decodeWidth.HasValue) img.DecodePixelWidth = decodeWidth.Value;
            if (decodeHeight.HasValue) img.DecodePixelHeight = decodeHeight.Value;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.UriCachePolicy = new(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
            img.UriSource = url;
            img.EndInit();
            return img;
        }

        // RenderTargetBitmap --> BitmapImage
        public static BitmapImage ConvertRenderTargetBitmapToBitmapImage(RenderTargetBitmap wbm)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.StreamSource = new MemoryStream(stream.ToArray()); //stream;
                bmp.EndInit();
                bmp.Freeze();
            }
            return bmp;
        }


        // RenderTargetBitmap --> BitmapImage
        public static BitmapImage RenderTargetBitmapToBitmapImage(RenderTargetBitmap rtb)
        {
            var renderTargetBitmap = rtb;
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        // BitmapImage --> byte[]
        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] bytearray = null;
            try
            {
                Stream smarket = bmp.StreamSource;
                if (smarket is not null && smarket.Length > 0)
                {
                    //Set the current location
                    smarket.Position = 0;
                    using BinaryReader br = new BinaryReader(smarket);
                    bytearray = br.ReadBytes((int)smarket.Length);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
            }
            return bytearray;
        }

        public static BitmapImage ToBitmapImage(this byte[] array) => ByteArrayToBitmapImage(array);
        // byte[] --> BitmapImage
        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using var ms = new MemoryStream(array);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
