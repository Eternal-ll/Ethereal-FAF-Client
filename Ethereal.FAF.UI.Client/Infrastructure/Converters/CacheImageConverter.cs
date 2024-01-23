using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class CacheImageConverter : IValueConverter
	{
		private IBackgroundImageCacheService _backgroundImageCacheService;
		private BitmapSource Empty;
        public CacheImageConverter()
        {
            var uri = new Uri("C:\\ProgramData\\FAForever\\cache\\empty.png");

            BitmapImage image = new();
            image.BeginInit();
            image.DecodePixelWidth = 60;
            image.DecodePixelHeight = 60;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = uri;
            image.EndInit();
            Empty = image;

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return new AsyncTaskTwo()
			{
				AsyncValue = Empty
			};
			_backgroundImageCacheService ??= App.Hosting.Services.GetService<IBackgroundImageCacheService>();
			var task = new AsyncTaskTwo();
			_backgroundImageCacheService.Load(value.ToString(), x =>
			{
                BitmapImage image = new();
                image.BeginInit();
                image.DecodePixelWidth = 60;
                image.DecodePixelHeight = 60;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(x);
                image.EndInit();
                //image.BeginInit();
                //image.DecodePixelWidth = 128;
                //image.DecodePixelHeight = 128;
                //image.CacheOption = BitmapCacheOption.OnLoad;
                //image.UriSource = new Uri(x);
                //image.EndInit();
                //image.Freeze();
                //if (image.CanFreeze)
                //{
                //    image.Freeze();
                //}
                task.AsyncValue = image;
            });
			return task;
        }

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
    }
    public class AsyncTaskTwo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object _AsyncValue;
        public object AsyncValue
        {
            get => _AsyncValue;
            set
            {
                if (!Equals(_AsyncValue, value))
                {
                    _AsyncValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AsyncValue)));
                }
            }

        }
    }
}
