using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class GetMapPreviewConverter : IValueConverter
    {
        private readonly IMapService MapService;

        public GetMapPreviewConverter()
        {
            MapService = App.Services.GetService<IMapService>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = MapService.GetMapPreview(new Uri("https://content.faforever.com/maps/previews/large/" + (string)value + ".png"), Folder.MapsLargePreviews);
            return t;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
