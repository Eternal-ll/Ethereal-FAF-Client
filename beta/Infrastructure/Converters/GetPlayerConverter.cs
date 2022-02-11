using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    /// <summary>
    /// Get player from PlayersService by login, id
    /// </summary>
    public class GetPlayerConverter : IValueConverter
    {
        private readonly IPlayersService PlayersService;
        public GetPlayerConverter()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
        }
        /// <summary>
        /// Get player from PlayersService by login, id. Returns Player instance or Null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var playerName = string.Empty;

            if (value is PlayerInfoMessage)
                return value;

            if (value is ChannelMessage channelMessage)
                if (channelMessage.From != null)
                    playerName = channelMessage.From;
                else return null;

            if (value is string player)
                playerName = player;

            return PlayersService.GetPlayer(playerName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
