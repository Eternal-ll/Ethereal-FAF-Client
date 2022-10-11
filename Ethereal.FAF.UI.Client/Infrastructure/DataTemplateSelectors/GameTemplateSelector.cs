using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer.Enums;
using System.Windows;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Infrastructure.DataTemplateSelectors
{
    public class GameTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IdleGame { get; set; }
        public DataTemplate LiveGame { get; set; }
        public DataTemplate MatchmakingGame { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var game = (Game)item;
            if (game.RatingType is RatingType.global)
            {
                if (game.State is GameState.Playing) return LiveGame;
                else return IdleGame;
            }
            return MatchmakingGame;
        }
    }
}
