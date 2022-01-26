using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class GetPlayerInfoConverter : IValueConverter
    {
        private readonly ILobbySessionService LobbySessionService;
        public GetPlayerInfoConverter()
        {
            LobbySessionService = App.Services.GetService<ILobbySessionService>();
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            
            var playerName = (string)value;
            if (LobbySessionService.PlayerNameToId.TryGetValue(playerName, out int id))
            {
                var playerInfo = LobbySessionService.Players[id];
                return playerInfo;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
