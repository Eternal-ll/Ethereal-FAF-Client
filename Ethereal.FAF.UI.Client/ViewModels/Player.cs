using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class Player : PlayerInfoMessage
    {
        private string _FlagUri;
        public string FlagUri => _FlagUri ??= "/Resources/Images/Flags/" + Country + ".png";

        [JsonPropertyName("players")]
        public new Player[] Players { get; set; }


        public int Global => Ratings.Global is null ? 0 : Ratings.Global.DisplayedRating;
        public int Ladder1v1 => Ratings.Ladder1V1 is null ? 0 : Ratings.Ladder1V1.DisplayedRating;
        public int Tmm2v2 => Ratings.Tmm2V2 is null ? 0 : Ratings.Tmm2V2.DisplayedRating;
        public int Tmm4v4 => Ratings.Tmm4V4FullShare is null ? 0 : Ratings.Tmm4V4FullShare.DisplayedRating;

        public RatingType DisplayRatingType { get; set; }
        public int UniversalRatingDisplay => DisplayRatingType switch
        {
            RatingType.global => Global,
            RatingType.ladder_1v1 => Ladder1v1,
            RatingType.tmm_4v4_full_share => Tmm4v4,
            RatingType.tmm_4v4_share_until_death => Tmm4v4,
            RatingType.tmm_2v2 => Tmm2v2,
        };



        // Player statuses

        #region IsLobbyConnected
        private bool _IsLobbyConnected;
        public bool IsLobbyConnected { get => _IsLobbyConnected; set => Set(ref _IsLobbyConnected, value); }
        #endregion

        #region IsIrcConnected
        private bool _IsIrcConnected;
        public bool IsIrcConnected { get => _IsIrcConnected; set => Set(ref _IsIrcConnected, value); }
        #endregion

        #region IsInGame
        public bool IsInGame => Game is not null;
        public bool IsPlaying => Game?.State is GameState.Playing;
        #endregion

        #region Game
        private Game _Game;
        new public Game Game
        {
            get => _Game;
            set
            {
                if (Set(ref _Game, value))
                {
                    OnPropertyChanged(nameof(IsInGame));
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }
        #endregion
    }
}
    