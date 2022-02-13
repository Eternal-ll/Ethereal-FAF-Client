using beta.Models.Server.Base;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace beta.Models.Server
{
    public enum GameState
    {
        None = 0,

        Open = 1,
        Playing = 2,
        Closed = 3,

        Host = 4,
        PrivateHost = 5,

        PrivateOpen = 6,
        PrivateClosed = 7,
        PrivatePlaying = 8,

        Playing5 = 9,
        PrivatePlaying5 = 10
    }

    public struct PlayerAvatar
    {
        public Uri url { get; set; }
        public string tooltip { get; set; }
    }

    public struct PlayerAlias
    {
        public string name { get; set; }
        public DateTime changeTime;
    }

    public enum PlayerState
    {
        IDLE = 1,
        PLAYING = 2,
        HOSTING = 3,
        JOINING = 4,
        SEARCHING_LADDER = 5,
        STARTING_AUTOMATCH = 6,
    }
    public interface IPlayer
    {
        public string login { get; set; }
    }
    public class UnknownPlayer : IPlayer
    {
        public string login { get; set; }
    }

    public class PlayerInfoMessage : ViewModel, IServerMessage, IPlayer
    {
        #region Custom properties

        #region Flag
        private ImageSource _Flag;
        public ImageSource Flag => _Flag ??= App.Current.Resources["Flag." + country] as ImageSource;
        #endregion

        #region GameState
        private GameState _GameState;
        public GameState GameState
        {
            get => _GameState;
            set
            {
                if (Set(ref _GameState, value))
                    OnPropertyChanged(nameof(GameStatusImage));
            }
        }
        #endregion

        #region GameStatusImage
        private object _GameStatusImage;
        public object GameStatusImage => GameState switch
        {
            GameState.None => null,
            GameState.Open => App.Current.Resources["PlayerGameStatus.open"],
            GameState.Playing => App.Current.Resources["PlayerGameStatus.playing"],
            GameState.Closed => null,
            GameState.Host => App.Current.Resources["PlayerGameStatus.host"],
            GameState.PrivateHost => null,
            GameState.PrivateOpen => null,
            GameState.PrivateClosed => null,
            GameState.PrivatePlaying => null,
            GameState.Playing5 => App.Current.Resources["PlayerGameStatus.playing5"],
            GameState.PrivatePlaying5 => null,
            _ => null
        };
        #endregion

        #region Note
        private string _Note;
        public string Note
        {
            get => _Note;
            set => Set(ref _Note, value);
        }
        #endregion

        #region Game
        private GameInfoMessage _Game;
        public GameInfoMessage Game
        {
            get => _Game;
            set
            {
                if (Set(ref _Game, value))
                {
                    if (value == null)
                    {
                        GameState = GameState.None;
                        return;
                    }
                    if (value.launched_at != null)
                    {
                        var timeDifference = DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(value.launched_at.Value);
                        GameState = timeDifference.TotalSeconds < 300 ? GameState.Playing5 : GameState.Playing;
                        return;
                    }

                    GameState = value.host.Equals(login, StringComparison.OrdinalIgnoreCase) ? GameState.Host : GameState.Open;
                }
            }
        }
        #endregion

        public DateTime? Updated { get; set; }

        #endregion

        public string command { get; set; }

        public int id { get; set; }
        public string login { get; set; }
        public string country { get; set; }
        public string clan { get; set; }
        public Dictionary<string, Rating> ratings { get; set; }
        public PlayerAvatar avatar { get; set; }
        public PlayerAlias[] names { get; set; }

        public PlayerInfoMessage[] players { get; set; }
    }
}
