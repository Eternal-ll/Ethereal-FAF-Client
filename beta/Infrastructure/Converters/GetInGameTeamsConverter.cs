using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class GetInGameTeamsConverter : IValueConverter
    {
        private readonly IGamesServices GamesServices;
        public GetInGameTeamsConverter()
        {
            GamesServices = App.Services.GetService<IGamesServices>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            //var game = (GameInfoMessage)value;
            //var teams = GamesServices.GetInGameTeams(game);
            return null ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
