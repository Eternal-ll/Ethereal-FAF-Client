using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
	public class CacheImageConverter : IValueConverter
	{
        private IFileCacheService _fileCacheService;
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            _fileCacheService ??= App.Hosting.Services.GetService<IFileCacheService>();
			return new AsyncTask(() =>
			{
				if (value is not string url) return value;
				if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return value;
				return _fileCacheService.Cache(url, default).Result;
			});
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public class AsyncTask : INotifyPropertyChanged
		{
			public AsyncTask(Func<object> valueFunc)
			{
				LoadValue(valueFunc);
			}

			private async Task LoadValue(Func<object> valueFunc)
			{
				AsyncValue = await Task<object>.Run(valueFunc);
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(AsyncValue)));
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public object AsyncValue { get; set; }
		}
	}
}
