using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace beta.Infrastructure.Services
{
    public class PlayersService : IPlayersService
    {
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage> PlayerUpdated;
        public event EventHandler<PlayerInfoMessage> PlayerLeft;

        public event EventHandler<PlayerInfoMessage> FriendConnected;
        public event EventHandler<PlayerInfoMessage> FoeConnected;
        public event EventHandler<PlayerInfoMessage> ClanmateConnected;

        public event EventHandler<PlayerInfoMessage> FriendJoinedToGame;
        public event EventHandler<PlayerInfoMessage> FriendLeftFromGame;
        public event EventHandler<PlayerInfoMessage> FriendFinishedGame;

        public event EventHandler<PlayerInfoMessage> FoeJoinedToGame;
        public event EventHandler<PlayerInfoMessage> FoeLeftFromGame;
        public event EventHandler<PlayerInfoMessage> FoeFinishedGame;

        public event EventHandler<PlayerInfoMessage> ClanmateJoinedToGame;
        public event EventHandler<PlayerInfoMessage> ClanmateLeftFromGame;
        public event EventHandler<PlayerInfoMessage> ClanmateFinishedGame;
        public event EventHandler<PlayerInfoMessage> SelfReceived;

        #region Properties

        #region Services
        private readonly ISessionService SessionService;
        private readonly IGamesService GamesService;
        private readonly ISocialService SocialService;
        private readonly INoteService NoteService;
        private readonly IIrcService IrcService;

        private readonly IFavouritesService FavoritesService;

        private readonly ILogger Logger;

        #endregion

        #region Players
        public List<PlayerInfoMessage> Players { get; } = new();
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
            IGamesService gamesService,
            ISocialService socialService,
            IIrcService ircService,
            IFavouritesService favoritesService,
            ILogger<PlayersService> logger)
        {
            SessionService = sessionService;
            NoteService = noteService;
            GamesService = gamesService;
            SocialService = socialService;
            IrcService = ircService;
            FavoritesService = favoritesService;
            Logger = logger;

            sessionService.PlayerReceived += OnPlayerReceived;
            sessionService.WelcomeDataReceived += OnWelcomeDataReceived;
            sessionService.PlayersReceived += OnPlayersReceived;

            gamesService.PlayersJoinedToGame += GamesService_PlayersJoinedToGame;
            gamesService.PlayersLeftFromGame += GamesService_PlayersLeftFromGame;
            GamesService.NewGameReceived += GamesService_NewGameReceived;

            socialService.PlayerdRelationshipChanged += SocialService_PlayerdRelationshipChanged;

            ircService.UserDisconnected += IrcService_UserDisconnected;
            ircService.UserLeft += IrcService_UserLeft;

            System.Windows.Application.Current.Exit += (s, e) =>
            {
                var players = Players;
                for (int i = 0; i < players.Count; i++)
                {
                    var player = players[i];
                    if (player.Note.Text?.Trim().Length > 0)
                    {
                        NoteService.Set(player.login, player.Note.Text);
                    }
                }
                NoteService.Save();
            };

            favoritesService.FavouriteAdded += FavoritesService_FavouriteAdded;
            favoritesService.FavouriteRemoved += FavoritesService_FavouriteRemoved;
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

        private void GamesService_NewGameReceived(object sender, GameInfoMessage e)
        {
            foreach (var team in e.teams)
                foreach (var login in team.Value)
                {
                    if (TryGetPlayer(login, out var player))
                    {
                        player.Game = e;
                        OnPlayerUpdated(player);
                    }
                    else
                    {
                        Logger.LogWarning($"New game received, but player not found");
                    }
                }
        }

        private void IrcService_UserLeft(object sender, Models.IRC.IrcUserLeft e)
        {
            if (e.Channel == "#aeolus")
            {
                var players = Players;
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].login.Equals(e.User, StringComparison.OrdinalIgnoreCase))
                    {
                        PlayerLeft?.Invoke(this, players[i]);
                        players.RemoveAt(i);
                    }
                }
            }
        }

        private void IrcService_UserDisconnected(object sender, string e)
        {
            if (TryGetPlayer(e, out var player))
            {
                Players.Remove(player);

                PlayerLeft?.Invoke(this, player);
                Logger.LogWarning("User disconnected");
            }
            else
            {
                Logger.LogWarning("User disconnected, but player instance not found");
            }
        }

        private void SocialService_PlayerdRelationshipChanged(object sender, KeyValuePair<int, PlayerRelationShip> e)
        {
            if (TryGetPlayer(e.Key, out var player))
            {
                player.RelationShip = e.Value;
                player.Updated = DateTime.UtcNow;
                OnPlayerUpdated(player);
                Logger.LogWarning($"Relationship with player {e.Key} updated to {e.Value}");
            }
            else
            {
                Logger.LogWarning($"Relationship with player {e.Key} changed to {e.Value} but instance not found");
            }
        }

        private void GamesService_PlayersJoinedToGame(object sender, KeyValuePair<GameInfoMessage, string[]> e)
        {
            for (int i = 0; i < e.Value.Length; i++)
            {
                if (TryGetPlayer(e.Value[i], out var player))
                {
                    player.Game = e.Key;
                    OnPlayerUpdated(player);
                    //Logger.LogInformation($"Player {player.login} joined to game and updated");
                }
                else
                {
                    Logger.LogWarning($"Player {e.Value[i]} joined to game and not updated");
                }
            }
        }

        private void GamesService_PlayersLeftFromGame(object sender, string[] e)
        {
            for (int i = 0; i < e.Length; i++)
            {
                if (TryGetPlayer(e[i], out var player))
                {
                    player.Game = null;
                    OnPlayerUpdated(player);
                    //Logger.LogInformation($"Player {player.login} left from game and updated");
                }
                else
                {
                    Logger.LogWarning($"Player {e[i]} left from game and not updated");
                }
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
            e.me.RelationShip = PlayerRelationShip.Me;
            Self = e.me;
            Players.Add(e.me);
            PlayerReceived?.Invoke(this, e.me);
            Logger.LogInformation("Self player instance received");
            SelfReceived?.Invoke(this, e.me);
        }

        private void HandlePlayerRelationship(PlayerInfoMessage player)
        {
            if (player.id == Self.id)
            {
                player.RelationShip = PlayerRelationShip.Me;
                Self = player;
                return;
            }
            if (IsClanMate(player.clan))
            {
                //player.RelationShip = PlayerRelationShip.Clan;
                player.IsClanmate = true;
            }
            player.RelationShip =
                IsFriend(player.id) ? PlayerRelationShip.Friend :
                IsFoe(player.id) ? PlayerRelationShip.Foe :
                PlayerRelationShip.None;
        }
        private void AddNewPlayer(PlayerInfoMessage player)
        {
            //TODO Workaround, because server sends two times self player instance.IDK why
            if (player.id == Self.id)
            {
                return;
            }
            HandlePlayerRelationship(player);
            Players.Add(player);
        }

        private void HandlePlayerData(PlayerInfoMessage player)
        {
            #region Add note about player

            player.Note = new();
            if (NoteService.TryGet(player.login, out var note))
            {
                player.Note.Text = note;
            }

            #endregion

            if (TryGetPlayer(player.id, out var matchedPlayer))
            {
                matchedPlayer.login = player.login;
                matchedPlayer.ratings = player.ratings;
                matchedPlayer.Updated = DateTime.UtcNow;

                OnPlayerUpdated(matchedPlayer);
                //Logger.LogInformation($"Player {player.id} updated");
            }
            else
            {
                OnNewPlayerReceived(player);
                //Logger.LogInformation($"New player {player.id} received");
            }
        }

        public PlayerInfoMessage GetPlayer(string idOrLogin)
        {
            var players = Players;
            int? id = int.TryParse(idOrLogin, out var res) ? res : null;
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if ((id.HasValue && id.Value == player.id) || player.login == idOrLogin)
                {
                    return player;
                }
            }
            return null;
        }

        public bool TryGetPlayer(int id, out PlayerInfoMessage player)
        {
            player = GetPlayer(id.ToString());
            if (player is null)
            {
                Logger.LogWarning($"Player not found: id {id}, players ({Players.Count})");
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
                Logger.LogWarning($"Player not found: login {login}, players ({Players.Count})");
            }
            return player is not null;
        }

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null)
        {
            var enumerator = Players.GetEnumerator();

            if (string.IsNullOrWhiteSpace(filter))
                while (enumerator.MoveNext())
                    if (relationShip.HasValue)
                        if (relationShip.Value == enumerator.Current.RelationShip)
                            yield return enumerator.Current;
                        else { }
                    else yield return enumerator.Current;
            else
                while (enumerator.MoveNext())
                    if (method == ComparisonMethod.STARTS_WITH)
                        if (enumerator.Current.login.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                            if (relationShip.HasValue)
                                if (relationShip.Value == enumerator.Current.RelationShip)
                                    yield return enumerator.Current;
                                else { }
                            else yield return enumerator.Current;
                        else { }
                    else if (enumerator.Current.login.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        if (relationShip.HasValue)
                            if (relationShip.Value == enumerator.Current.RelationShip)
                                yield return enumerator.Current;
                            else { }
                        else yield return enumerator.Current;
        }

        private bool IsClanMate(string clan) => Self.clan != null && Self.clan == clan;
        public bool IsClanMate(PlayerInfoMessage player) => IsClanMate(player.clan);//player.clan != null ? IsClanMate(player.clan) : false;

        private bool IsFriend(int id)
        {
            var friends = FriendsIds;
            if (friends.Count == 0) return false;

            for (int i = 0; i < friends.Count; i++)
            {
                if (friends[i] == id) return true;
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
        private void OnNewPlayerReceived(PlayerInfoMessage e)
        {
            AddNewPlayer(e);
            PlayerReceived?.Invoke(this, e);

            switch (e.RelationShip)
            {
                case PlayerRelationShip.Friend:
                    FriendConnected?.Invoke(this, e);
                    break;
                case PlayerRelationShip.Foe:
                    FoeConnected?.Invoke(this, e);
                    break;
                case PlayerRelationShip.Clan:
                    ClanmateConnected?.Invoke(this, e);
                    break;
            }
        }

        //protected virtual void OnPlayersUpdated(PlayerInfoMessage[] e) => 
        //protected virtual void OnPlayersReceived(PlayerInfoMessagep[] e) =>
    }
}
