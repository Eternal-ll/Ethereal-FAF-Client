using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.Server
{
    public enum GameState : byte
    {
        None = 0,

        Open = 1,
        Playing = 2,
        Closed = 3,

        Host = 4,

        Playing5 = 9
    }

    public class PlayerAvatar
    {
        public Uri url { get; set; }
        
        public string tooltip { get; set; }
    }

    // unused
    public struct PlayerAlias
    {
        public string name { get; set; }
        public DateTime changeTime;
    }

    // unused
    public enum PlayerState
    {
        IDLE = 1,
        PLAYING = 2,
        HOSTING = 3,
        JOINING = 4,
        SEARCHING_LADDER = 5,
        STARTING_AUTOMATCH = 6,
    }
    
    public static class PlayerInfoExtensions
    {
        public static PlayerInfoMessage Update(this PlayerInfoMessage orig, PlayerInfoMessage newP)
        {
            orig.login = newP.login;
            orig.avatar = newP.avatar;

            orig.ratings = newP.ratings;
            orig.Updated = DateTime.Now;
            return orig;
        }
    }

    public class PlayerInfoMessage : ViewModel, IServerMessage, IPlayer
    {
        public ServerCommand Command { get; set; }

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
        public object GameStatusImage => GameState switch
        {
            GameState.Open => App.Current.Resources["PlayerGameStatus.open"],
            GameState.Playing => App.Current.Resources["PlayerGameStatus.playing"],
            GameState.Host => App.Current.Resources["PlayerGameStatus.host"],
            GameState.Playing5 => App.Current.Resources["PlayerGameStatus.playing5"],
            GameState.None => null,
            _=> null
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

        #region Avatar
        private BitmapImage _Avatar;
        public BitmapImage Avatar
        {
            get => _Avatar;
            set => Set(ref _Avatar, value);
        }
        #endregion

        // SOCIAL
        #region Note
        private PlayerNote _PlayerNote;
        public PlayerNote PlayerNote
        {
            get => _PlayerNote;
            set=>Set(ref _PlayerNote, value);
        }
        #endregion

        // CHAT PROPERTIES FOR SORTING
        #region IsChatModerator
        private bool _IsChatModerator;
        public bool IsChatModerator
        {
            get => _IsChatModerator;
            set => Set(ref _IsChatModerator, value);
        }
        #endregion

        // SOCIAL
        // CHAT PROPERTIES FOR SORTING
        #region RelationShip
        private PlayerRelationShip _RelationShip;
        public PlayerRelationShip RelationShip
        {
            get => _RelationShip;
            set => Set(ref _RelationShip, value);
        }
        #endregion

        public DateTime? Updated { get; set; }

        #endregion

        public string command { get; set; }

        public int id { get; set; }

        #region login
        private string _login;
        public string login
        {
            get => _login;
            set => Set(ref _login, value);
        } 
        #endregion

        public string country { get; set; }
        public string clan { get; set; }

        #region ratings
        private Dictionary<string, Rating> _ratings;
        public Dictionary<string, Rating> ratings
        {
            get => _ratings;
            set
            {
                if (value != _ratings)
                {
                    if (_ratings != null)
                        foreach (var item in value)
                        {
                            if (_ratings.TryGetValue(item.Key, out Rating rating))
                            {
                                if (rating == null) continue;
                                rating.RatingDifference[0] += item.Value.rating[0] - rating.rating[0];
                                rating.RatingDifference[1] += item.Value.rating[1] - rating.rating[1];
                                rating.GamesDifference++;
                            }
                            else
                            {
                                _ratings.Add(item.Key, item.Value);

                                _ratings[item.Key].RatingDifference[0] += item.Value.rating[0];
                                _ratings[item.Key].RatingDifference[1] += item.Value.rating[1];
                                _ratings[item.Key].GamesDifference++;
                            };
                        }
                    else _ratings = value;
                    OnPropertyChanged(nameof(ratings));
                }
            }
        } 
        #endregion
        
        // TODO RENAME to PlayerAvatarData?
        public PlayerAvatar avatar { get; set; }

        public PlayerInfoMessage[] players { get; set; }
    }
}
