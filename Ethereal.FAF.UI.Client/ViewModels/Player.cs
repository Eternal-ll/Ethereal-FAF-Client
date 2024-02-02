using CommunityToolkit.Mvvm.ComponentModel;
using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class Player : ObservableObject
    {
        #region OriginalProperties
        public long Id { get; set; }
        public string Login { get; set; }
        public PlayerState State { get; set; }
        public bool IsOffline => State == PlayerState.offline;
        public PlayerAvatar Avatar { get; set; }
        public string Country { get; set; }
        public string Clan { get; set; }
        public Ratings Ratings { get; set; }

        #endregion

        public string LoginWithClan => $"{(Clan is null ? null : '[' + Clan + "] ")}{Login}";

        public int Global => Ratings.Global is null ? 0 : Ratings.Global.DisplayedRating;
        public int Ladder1v1 => Ratings.Ladder1V1 is null ? 0 : Ratings.Ladder1V1.DisplayedRating;
        public int Tmm2v2 => Ratings.Tmm2V2 is null ? 0 : Ratings.Tmm2V2.DisplayedRating;
        public int Tmm4v4 => Ratings.Tmm4V4FullShare is null ? 0 : Ratings.Tmm4V4FullShare.DisplayedRating;
        public int GlobalGames => Ratings.Global is null ? 0 : Ratings.Global.number_of_games;
        public int Ladder1v1Games => Ratings.Ladder1V1 is null ? 0 : Ratings.Ladder1V1.number_of_games;
        public int Tmm2v2Games => Ratings.Tmm2V2 is null ? 0 : Ratings.Tmm2V2.number_of_games;
        public int Tmm4v4Games => Ratings.Tmm4V4FullShare is null ? 0 : Ratings.Tmm4V4FullShare.number_of_games;

        public RatingType DisplayRatingType { get; set; }
        public int UniversalRatingDisplay => DisplayRatingType switch
        {
            RatingType.global => Global,
            RatingType.ladder_1v1 => Ladder1v1,
            RatingType.tmm_4v4_full_share => Tmm4v4,
            RatingType.tmm_4v4_share_until_death => Tmm4v4,
            RatingType.tmm_2v2 => Tmm2v2,
        };

        public int UniversalGameRatingDisplay => Game is null ? UniversalRatingDisplay :
            Game.RatingType switch
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
        public bool IsLobbyConnected { get => _IsLobbyConnected; set => SetProperty(ref _IsLobbyConnected, value); }
        #endregion

        public bool IsIrcConnected => !string.IsNullOrWhiteSpace(IrcUsername);
        #region IrcUsername
        private string _IrcUsername;
        public string IrcUsername
        {
            get => _IrcUsername;
            set
            {
                if (SetProperty(ref _IrcUsername, value))
                {
                    OnPropertyChanged(nameof(IsIrcConnected));
                }
            }
        }
        #endregion

        #region IsInGame
        public bool IsInGame => Game is not null;
        public bool IsPlaying => Game?.State is GameState.Playing;
        #endregion

        #region Game
        private Game _Game;
        public Game Game
        {
            get => _Game;
            set
            {
                if (SetProperty(ref _Game, value))
                {
                    OnPropertyChanged(nameof(IsInGame));
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(UniversalGameRatingDisplay));
                }
            }
        }
        #endregion
    }
}
    