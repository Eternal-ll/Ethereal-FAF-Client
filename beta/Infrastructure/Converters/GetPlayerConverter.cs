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
            if (value is null) return null;

            var playerName = string.Empty;

            if (value is PlayerInfoMessage)
                return value;

            if (value is string player)
                playerName = player;

            bool isChatMod = playerName.StartsWith('@');

            if (isChatMod)
            {
                playerName = playerName.Substring(1);
            }

            var playerInstance = PlayersService.GetPlayer(playerName);
            if (playerInstance is null)
            {
                return new UnknownPlayer()
                {
                    login = playerName,
                    IsChatModerator = isChatMod
                };
            }
            playerInstance.IsChatModerator = isChatMod;

            return playerInstance;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
