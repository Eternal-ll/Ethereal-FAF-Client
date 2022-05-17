using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace beta.Infrastructure.Services
{
    public class PlayersService : IPlayersService
    {
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage> PlayerUpdated;
        public event EventHandler<PlayerInfoMessage> PlayerLeft;

        public event EventHandler<PlayerInfoMessage> SelfReceived;

        #region Properties

        #region Services
        private readonly ISessionService SessionService;
        //private readonly IGamesService GamesService;
        private readonly ISocialService SocialService;
        private readonly INoteService NoteService;
        private readonly IIrcService IrcService;

        private readonly IFavouritesService FavoritesService;

        private readonly ILogger Logger;

        #endregion

        #region Players
        public System.Collections.Concurrent.ConcurrentDictionary<int, PlayerInfoMessage> PlayersDic { get; } = new();
        public PlayerInfoMessage[] Players => PlayersDic.Select(x => x.Value).ToArray();
        //private readonly Dictionary<int, int> PlayerUIDToId = new();
        //private readonly Dictionary<string, int> PlayerLoginToId = new();
        //private List<PlayerInfoMessage> PlayersLogins { get; } = new();

        #endregion

        private List<int> FriendsIds { get; set; } = new();
        private List<int> FoesIds { get; set; } = new();
        
        public PlayerInfoMessage[] CachedPlayers => throw new NotImplementedException();
        #endregion

        public PlayerInfoMessage Self { get; private set; }

        #region Ctor

        public PlayersService(
            ISessionService sessionService,
            INoteService noteService,
            //IGamesService gamesService,
            ISocialService socialService,
            IIrcService ircService,
            IFavouritesService favoritesService,
            ILogger<PlayersService> logger)
        {
            SessionService = sessionService;
            NoteService = noteService;
            //GamesService = gamesService;
            SocialService = socialService;
            IrcService = ircService;
            FavoritesService = favoritesService;
            Logger = logger;

            sessionService.PlayerReceived += OnPlayerReceived;
            sessionService.WelcomeDataReceived += OnWelcomeDataReceived;
            sessionService.PlayersReceived += OnPlayersReceived;
            sessionService.StateChanged += SessionService_StateChanged;

            //gamesService.PlayersJoinedToGame += GamesService_PlayersJoinedToGame;
            //gamesService.PlayersLeftFromGame += GamesService_PlayersLeftFromGame;
            //GamesService.NewGameReceived += GamesService_NewGameReceived;

            socialService.PlayerdRelationshipChanged += SocialService_PlayerdRelationshipChanged;

            ircService.UserDisconnected += IrcService_UserDisconnected; 
            ircService.UserLeft += IrcService_UserLeft;

            System.Windows.Application.Current.Exit += (s, e) =>
            {
                var players = PlayersDic;
                //for (int i = 0; i < players.Count; i++)
                //{
                //    var player = players[i];
                //    if (player.Note?.Text?.Trim().Length > 0)
                //    {
                //        NoteService.Set(player.login, player.Note.Text);
                //    }
                //}
                NoteService.Save();
            };

            favoritesService.FavouriteAdded += FavoritesService_FavouriteAdded;
            favoritesService.FavouriteRemoved += FavoritesService_FavouriteRemoved;

            sessionService.SocialDataReceived += SessionService_SocialDataReceived;
        }

        private void SessionService_SocialDataReceived(object sender, SocialData e)
        {
            FriendsIds = new(e.friends);
            FoesIds = new(e.foes);
        }

        private void SessionService_StateChanged(object sender, SessionState e)
        {
            if (e == SessionState.Disconnected)
            {
                PlayersDic.Clear();
            }
        }

        private void FavoritesService_FavouriteRemoved(object sender, int e)
        {
            if (TryGetPlayer(e, out var player))
            {
                player.IsFavourite = false;
                OnPlayerUpdated(player);
            }
        }

        private void FavoritesService_FavouriteAdded(object sender, int e)
        {
            if (TryGetPlayer(e, out var player))
            {
                player.IsFavourite = true;
                OnPlayerUpdated(player);
            }
        }

        private void IrcService_UserLeft(object sender, Models.IRC.IrcUserLeft e)
        {
            // TODO
            return;
            if (e.Channel == "#aeolus")
            {
                var players = Players;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].login.Equals(e.User, StringComparison.OrdinalIgnoreCase))
                    {
                        PlayerLeft?.Invoke(this, players[i]);
                        PlayersDic.Remove(players[i].id, out var removed);
                    }
                }
            }
        }

        private void IrcService_UserDisconnected(object sender, string e)
        {
            if (TryGetPlayer(e, out var player))
            {
                PlayersDic.Remove(player.id, out var removed);

                PlayerLeft?.Invoke(this, player);
            }
            else
            {
                Logger.LogError("User disconnected, but player instance not found");
            }
        }

        private void SocialService_PlayerdRelationshipChanged(object sender, KeyValuePair<int, PlayerRelationShip> e)
        {
            if (TryGetPlayer(e.Key, out var player))
            {
                var oldRelationship = player.RelationShip;
                player.RelationShip = e.Value;
                player.Updated = DateTime.UtcNow;
                OnPlayerUpdated(player);
                Logger.LogWarning($"Relationship with player {e.Key} updated to {e.Value}");

                if (player.Game is not null)
                {
                    if (e.Value == PlayerRelationShip.Friend)
                    {
                        player.Game.Friends++;
                    }
                    if (e.Value == PlayerRelationShip.Foe)
                    {
                        player.Game.Foes++;
                    }
                    if (oldRelationship == PlayerRelationShip.Friend)
                    {
                        player.Game.Friends--;
                    }
                    if (oldRelationship == PlayerRelationShip.Foe)
                    {
                        player.Game.Foes--;
                    }
                }
            }
            else
            {
                Logger.LogWarning($"Relationship with player {e.Key} changed to {e.Value} but instance not found");
            }
        }
        #endregion

        #region Event listeners

        private void OnPlayersReceived(object sender, PlayerInfoMessage[] e)
        {
            for (int i = 0; i < e.Length; i++)
            {
                AddNewPlayer(e[i]);
            }
            PlayersReceived?.Invoke(this, e);
        }

        private void OnWelcomeDataReceived(object sender, WelcomeData e)
        {
            Logger.LogInformation("Self player instance received");
            e.me.RelationShip = PlayerRelationShip.Me;

            //e.me.Note = new();
            //if (NoteService.TryGet(e.me.login, out var note))
            //{
            //    e.me.Note.Text = note;
            //}
            Self = e.me;
            PlayersDic.TryAdd(e.me.id, Self);
            //PlayerReceived?.Invoke(this, Self);
            //SelfReceived?.Invoke(this, Self);
        }

        private void HandlePlayerRelationship(PlayerInfoMessage player)
        {
            player.IsClanmate = IsClanMate(player.clan);
            
            player.RelationShip =
                IsFriend(player.id) ? PlayerRelationShip.Friend :
                IsFoe(player.id) ? PlayerRelationShip.Foe :
                PlayerRelationShip.None;
        }
        private void AddNewPlayer(PlayerInfoMessage player)
        {
            player.Note = new();
            if (NoteService.TryGet(player.login, out var note))
            {
                player.Note.Text = note;
            }

            if (player.id == Self.id)
            {
                player.RelationShip = PlayerRelationShip.Me;
                Self = player;
                SelfReceived?.Invoke(this, Self);
                //Logger.LogWarning($"Self received, but didnt passed");
                //return;
            }
            else
            {
                HandlePlayerRelationship(player);
            }

            if (!PlayersDic.TryAdd(player.id, player))
            {
                if (PlayersDic.TryGetValue(player.id, out var matchedPlayer))
                {
                    Logger.LogWarning($"Tried to add player that is already in dictionary: {player.id} - {player.login}, updated instead");
                    matchedPlayer.login = player.login;
                    matchedPlayer.ratings = player.ratings;
                    matchedPlayer.Updated = DateTime.UtcNow;

                    OnPlayerUpdated(matchedPlayer);
                }
                else
                {
                    Logger.LogWarning($"Tried to add player that is already in dictionary: {player.id} - {player.login}, but update didnt work");
                }
            }
            else
            {
                PlayerReceived?.Invoke(this, player);
            }
        }

        private void HandlePlayerData(PlayerInfoMessage player)
        {
            if (PlayersDic.TryGetValue(player.id, out var matchedPlayer))
            {
                matchedPlayer.login = player.login;
                matchedPlayer.ratings = player.ratings;
                matchedPlayer.Updated = DateTime.UtcNow;
                OnPlayerUpdated(matchedPlayer);
            }
            else
            {
                player.Note = new();
                if (NoteService.TryGet(player.login, out var note))
                {
                    player.Note.Text = note;
                }
                AddNewPlayer(player);
            }
        }

        public PlayerInfoMessage GetPlayer(string idOrLogin)
        {
            int? id = int.TryParse(idOrLogin, out var res) ? res : null;
            if (id.HasValue)
            {
                PlayerInfoMessage player = null;
                PlayersDic.TryGetValue(id.Value, out player);
                return player;
            }
            idOrLogin = idOrLogin.ToLower();
            var players = Players;
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player.login.ToLower() == idOrLogin)
                {
                    return player;
                }
            }
            return null;
        }

        public bool TryGetPlayer(int id, out PlayerInfoMessage player)
        {
            PlayersDic.TryGetValue(id, out player);
            if (player is null)
            {
                //Logger.LogWarning($"Player not found: id {id}, players ({PlayersDic.Count})");
            }
            return player is not null;
        }
        private void OnPlayerReceived(object sender, PlayerInfoMessage player) => HandlePlayerData(player);

        #endregion

        public bool TryGetPlayer(string login, out PlayerInfoMessage player)
        {
            player = GetPlayer(login);
            if (player is null)
            {
                //Logger.LogWarning($"Player not found: login {login}, players ({PlayersDic.Count})");
            }
            return player is not null;
        }

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null)
        {
            var players = Players;
            return null;
        }

        private bool IsClanMate(string clan) => Self.clan != null && Self.clan == clan;
        public bool IsClanMate(PlayerInfoMessage player) => IsClanMate(player.clan);//player.clan != null ? IsClanMate(player.clan) : false;

        private bool IsFriend(int id)
        {
            var friends = FriendsIds;
            if (friends.Count == 0) return false;

            for (int i = 0; i < friends.Count; i++)
            {
                if (friends[i] == id)
                    return true;
            }
            return false;
        }
        public bool IsFriend(PlayerInfoMessage player) => IsFriend(player.id);

        private bool IsFoe(int id)
        {
            var foes = FoesIds;
            if (foes.Count == 0) return false;

            for (int i = 0; i < foes.Count; i++)
            {
                if (foes[i] == id) return true;
            }
            return false;
        }
        public bool IsFoe(PlayerInfoMessage player) => IsFoe(player.id);

        public bool AddGameToPlayer(string login, GameInfoMessage game)
        {
            if (string.IsNullOrWhiteSpace(login)) return false;
            if (game is null) return false;

            if (TryGetPlayer(login, out var player))
            {
                if (player.Game is not null)
                {
                    // TODO log if this happens
                }
                player.Game = game;
                return true;
            }
            return false;
        }

        public bool AddGameToPlayers(string[] logins, GameInfoMessage game)
        {
            if (logins is null || logins.Length == 0) return false;
            if (game is null) return false;

            bool done = true;
            foreach (string login in logins)
            {
                if (!AddGameToPlayer(login, game))
                {
                    done = false;
                }
            }
            return done;
        }

        public bool RemoveGameFromPlayer(string login, long? uid = null)
        {
            if (string.IsNullOrWhiteSpace(login)) return false;

            if (TryGetPlayer(login, out var player))
            {
                if (player.Game is null)
                {
                    // TODO log if this happens
                }

                if (uid.HasValue)
                {
                    if (player.Game.uid != uid.Value)
                    {
                        // TODO log if this happens
                    }
                    player.Game = null;
                }
                else
                {
                    player.Game = null;
                }
                return true;
            }
            return false;
        }

        public bool RemoveGameFromPlayers(string[] logins, long? uid = null)
        {
            if (logins is null || logins.Length == 0) return false;

            bool done = true;
            foreach (string login in logins)
            {
                if (!RemoveGameFromPlayer(login, uid))
                {
                    done = false;
                }
            }
            return done;
        }

        protected virtual void OnPlayerUpdated(PlayerInfoMessage e) => PlayerUpdated?.Invoke(this, e);
    }
}
