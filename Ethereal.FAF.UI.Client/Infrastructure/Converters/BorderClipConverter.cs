using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class ToPlayerConverter : IValueConverter
    {
        private PlayersViewModel Model;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Model ??= App.Hosting.Services.GetService<PlayersViewModel>();
            var players = Model.Players;
            if (value is string id && long.TryParse(id, out var idParsed))
            {
                return players.FirstOrDefault(p => p.Id == idParsed);
            }
            return players.FirstOrDefault(p => p.Login == value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SmallPreviewImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string cache) return null;
            if (cache.Contains("neroxis") && !File.Exists(cache)) return null;
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelHeight = 128;
            bitmap.DecodePixelWidth = 128;
            //bitmap.CacheOption = BitmapCacheOption.OnDemand;
            bitmap.UriSource = new Uri(cache);
            bitmap.UriCachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheOnly);
            bitmap.EndInit();
            //bitmap.DownloadCompleted += Bitmap_DownloadCompleted;
            bitmap.Freeze();
            return null;
        }

        private void Bitmap_DownloadCompleted(object sender, EventArgs e)
        {
            ((BitmapImage)sender).Freeze();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BorderClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 &&
                values[0] is double &&
                values[1] is double &&
                values[2] is CornerRadius)
            {
                var width = (double)values[0];
                var height = (double)values[1];
                if (width < double.Epsilon || height < double.Epsilon)
                {
                    return Geometry.Empty;
                }
                var radius = (CornerRadius)values[2];
                // Actually we need more complex geometry, when CornerRadius has different values.
                // But let me not to take this into account, and simplify example for a common value.
                var clip = new RectangleGeometry(new Rect(0, 0, width, height), radius.TopLeft, radius.TopLeft);
                clip.Freeze();
                return clip;
            }
            return DependencyProperty.UnsetValue;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
