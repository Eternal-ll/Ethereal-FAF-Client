using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer.Enums;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Infrastructure.DataTemplateSelectors
{
    public class GamePlayerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WithAvatar { get; set; }
        public DataTemplate WithoutAvatar { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var gameplayer = (GamePlayer)item;
            if (gameplayer.Player?.Avatar is not null) return WithAvatar;
            return WithoutAvatar;
        }
    }
    public class GameTemplateSelector : DataTemplateSelector
    {
        public static bool LadderCompact;
        public DataTemplate IdleGame { get; set; }
        public DataTemplate LiveGame { get; set; }
        public DataTemplate MatchmakingGame { get; set; }

        public DataTemplate LadderGameOne { get; set; }
        public DataTemplate LadderGameCompact { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var game = (Game)item;
            if (game.RatingType is RatingType.global)
            {
                if (game.State is GameState.Playing) return LiveGame;
                else return IdleGame;
            }
            switch (game.RatingType)
            {
                case RatingType.global:
                    break;
                case RatingType.ladder_1v1:
                    return LadderCompact ? LadderGameCompact : LadderGameOne;
                case RatingType.tmm_2v2:
                    break;
                case RatingType.tmm_4v4_full_share:
                    break;
                case RatingType.tmm_4v4_share_until_death:
                    break;
                default:
                    break;
            }
            return MatchmakingGame;
        }
    }
    public class MatchmakingGameTeamTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WithAvatarImage { get; set; }
        public DataTemplate WithoutAvatarImage { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var team = (GameTeam)item;
            if (team.GamePlayers.Any(p => p.Player?.Avatar is not null))
            {
                return WithAvatarImage;
            }
            return WithoutAvatarImage;
        }
    }
}
