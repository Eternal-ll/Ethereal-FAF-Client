using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.Properties;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.Server
{
    public static class PlayerInfoExtensions
    {
        public static PlayerInfoMessage Update(this PlayerInfoMessage orig, PlayerInfoMessage newP)
        {
            orig.login = newP.login;

            // updates from AvatarService.UpdatePlayerAvatarAsync
            //orig.Avatar = newP.Avatar;

            orig.ratings = newP.ratings;
            orig.Updated = DateTime.Now;
            return orig;
        }
        public static void FillTest(this PlayerInfoMessage player)
        {
            player.login = Settings.Default.PlayerNick;
            player.id = int.Parse(Settings.Default.PlayerId.ToString());
            player.ratings = new()
            {
                { "global", new() { rating = new double[] { 9999, 1 } } },
                { "ladder_1v1", new() { rating = new double[] { 9999, 1 } } },
                { "tmm_2v2", new() { rating = new double[] { 9999, 1 } } },
                { "tmm_4v4_full_share", new() { rating = new double[] { 9999, 1 } } },
                { "tmm_4v4_share_until_death", new() { rating = new double[] { 9999, 1 } } },
            };
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

        private BitmapImage _AvatarImage;
        public ImageSource AvatarImage
        {
            get
            {
                if (Avatar is null) return null;
                if (_AvatarImage is null)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.DecodePixelWidth = 40;
                    img.DecodePixelHeight = 20;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriCachePolicy = new(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
                    img.UriSource = Avatar.Url;
                    img.EndInit();
                    _AvatarImage = img;
                }
                return _AvatarImage;
            }
        }

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

        #region IsFavourite
        private bool _IsFavourite;
        public bool IsFavourite
        {
            get => _IsFavourite;
            set => Set(ref _IsFavourite, value);
        }
        #endregion

        #region Game
        private GameInfoMessage _Game;
        public GameInfoMessage Game
        {
            get => _Game;
            set
            {
                if (login == "Eternal-")
                {

                }
                if (Set(ref _Game, value))
                {
                    if (value is null)
                    {
                        GameState = GameState.None;
                        return;
                    }
                    if (value.launched_at is not null)
                    {
                        var timeDifference = DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(value.launched_at.Value);
                        GameState = timeDifference.TotalSeconds < 300 ? GameState.Playing5 : GameState.Playing;
                        return;
                    }

                    OnPropertyChanged(nameof(DisplayedRating));
                    GameState = value.host.Equals(login, StringComparison.OrdinalIgnoreCase) ? GameState.Host : GameState.Open;
                }
            }
        }
        #endregion

        // SOCIAL
        #region Note
        //TODO
        private PlayerNoteVM _Note;
        public PlayerNoteVM Note
        {
            get => _Note;
            set => Set(ref _Note, value);
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

        #region IsClanmate
        private bool _IsClanmate;
        public bool IsClanmate
        {
            get => _IsClanmate;
            set => Set(ref _IsClanmate, value);
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

        #region IrcName
        private string _IrcName;
        public string IrcName
        {
            get => _IrcName;
            set => Set(ref _IrcName, value);
        }
        #endregion



        public string LoginWithClan => clan == null ? login : $"[{clan}] {login}";

        public DateTime Updated { get; set; }

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

        public Dictionary<string, ObservableCollection<Rating>> RatingHistory = new()
        {
            { "global", new() },
            { "ladder_1v1", new() },
            { "tmm_2v2", new() },
            { "tmm_4v4_full_share", new() },
            { "tmm_4v4_share_until_death", new() },
        };

        #region ratings
        private Dictionary<string, Rating> _ratings;
        public Dictionary<string, Rating> ratings
        {
            get => _ratings;
            set
            {
                if (value != _ratings)
                {
                    
                    if (_ratings is not null)
                        foreach (var item in value)
                        {
                            if (_ratings.TryGetValue(item.Key, out Rating rating))
                            {
                                if (rating is null) continue;
                                rating.RatingDifference[0] += item.Value.rating[0] - rating.rating[0];
                                rating.RatingDifference[1] += item.Value.rating[1] - rating.rating[1];
                                rating.GamesDifference++;
                                RatingHistory[item.Key].Add(item.Value);
                            }
                            else
                            {

                                item.Value.RatingDifference[0] += item.Value.rating[0];
                                item.Value.RatingDifference[1] += item.Value.rating[1];
                                item.Value.GamesDifference++;
                                _ratings.Add(item.Key, item.Value);
                                RatingHistory[item.Key].Add(item.Value);
                            };
                        }
                    else _ratings = value;
                    OnPropertyChanged(nameof(ratings));
                    OnPropertyChanged(nameof(DisplayedRating));
                }
            }
        }
        #endregion

        public Rating DisplayedRating
        {
            get
            {
                if (ratings is null) return null;
                string ratingType = "global";
                if (Game is not null)
                {
                    ratingType = Game.RatingType;
                }
                return ratings.TryGetValue(ratingType, out var rating) ? rating : new()
                {
                    number_of_games = 0,
                    rating = new double[] { 1500, 500 },
                    RatingDifference = new double[] {1500, 500}
                };
            }
        }

        // TODO RENAME to PlayerAvatarData?

        #region Avatar
        private PlayerAvatar _Avatar;
        [JsonPropertyName("avatar")]
        public PlayerAvatar Avatar
        {
            get => _Avatar;
            set => Set(ref _Avatar, value);
        } 
        #endregion

        public PlayerInfoMessage[] players { get; set; }
    }
}
