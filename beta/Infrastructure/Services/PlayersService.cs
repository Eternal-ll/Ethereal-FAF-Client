using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class PlayersService : ViewModel, IPlayersService
    {
        public event EventHandler<PlayerInfoMessage> MeReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;

        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IAvatarService AvatarService;
        private readonly INoteService NoteService;

        #endregion

        #region Players

        public readonly ObservableCollection<PlayerInfoMessage> _Players = new();
        public ObservableCollection<PlayerInfoMessage> Players => _Players;

        private readonly Dictionary<int, int> PlayerUIDToId = new();
        private readonly Dictionary<string, int> PlayerLoginToId = new();


        #endregion

        private List<int> FriendsIds { get; set; } = new();
        private List<int> FoesIds { get; set; } = new();

        #endregion

        #region Ctor

        public PlayersService(
            ISessionService sessionService,
            IAvatarService avatarService,
            INoteService noteService)
        {
            SessionService = sessionService;
            AvatarService = avatarService;
            NoteService = noteService;

            sessionService.PlayerReceived += OnPlayerReceived;
            sessionService.SocialDataReceived += OnNewSocialDataReceived;
            sessionService.WelcomeDataReceived += OnWelcomeDataReceived;
            sessionService.PlayersReceived += OnPlayersReceived;

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
        }

        private void OnPlayersReceived(object sender, PlayerInfoMessage[] e) => Task.Run(async () =>
        {
            foreach (var player in e) await HandlePlayerData(player);
        });


        #endregion

        public PlayerInfoMessage Me { get; private set; }

        #region Event listeners
        private void OnWelcomeDataReceived(object sender, WelcomeData e)
        {
            Me = e.me;
            Me.RelationShip = PlayerRelationShip.Me;

            // TODO
            if (Players.Count == 0)
            {
                Players.Add(Me);
                PlayerLoginToId.Add(Me.login.ToLower(), 0);
                PlayerUIDToId.Add(Me.id, 0);
            }
            MeReceived?.Invoke(this, Me);
        }
        private void OnNewSocialDataReceived(object sender, SocialData e)
        {
            FriendsIds = e.friends;
            FoesIds = e.foes;

            var friendsIds = e.friends;
            var foesIds = e.foes;

            // TODO REWRITE?
            if (friendsIds is not null && friendsIds.Count > 0)
                for (int i = 0; i < friendsIds.Count; i++)
                    if (PlayerUIDToId.TryGetValue(friendsIds[i], out var id))
                    {
                        var player = Players[id];
                        if (friendsIds[i] == player.id)
                        {
                            player.RelationShip = PlayerRelationShip.Friend;
                            return;
                        }
                    }

            if (foesIds is not null && foesIds.Count > 0)
                for (int i = 0; i < foesIds.Count; i++)
                    if (PlayerUIDToId.TryGetValue(friendsIds[i], out var id))
                    {
                        var player = Players[id];
                        if (foesIds[i] == player.id)
                        {
                            player.RelationShip = PlayerRelationShip.Foe;
                            return;
                        }
                    }
        }
        private async Task HandlePlayerData(PlayerInfoMessage player)
        {
            var players = _Players;
            #region Add note about player

            if (NoteService.TryGet(player.login, out var note))
            {
                player.Note.Text = note;
            }

            #endregion

            #region Matching clanmates & friends & foes

            var friendsIds = FriendsIds;
            var foesIds = FoesIds;
            var me = Me;

            if (player.id != me.id)
            {
                if (IsClanMate(player.clan))
                {
                    player.RelationShip = PlayerRelationShip.Clan;
                }
                else if (IsFriend(player.id))
                {
                    player.RelationShip = PlayerRelationShip.Friend;
                }
                else if (IsFoe(player.id))
                {
                    player.RelationShip = PlayerRelationShip.Foe;
                }
            }
            else
            {
                player.RelationShip = PlayerRelationShip.Me;
            }

            #endregion

            if (PlayerUIDToId.TryGetValue(player.id, out var id))
            {
                var matchedPlayer = players[id];

                await AvatarService.UpdatePlayerAvatarAsync(matchedPlayer, player.Avatar);
                //await Task.Run(async () => await AvatarService.UpdatePlayerAvatarAsync(matchedPlayer, player.Avatar));

                matchedPlayer.Update(player);
            }
            else
            {
                int count = players.Count;
                await AvatarService.UpdatePlayerAvatarAsync(player, player.Avatar);
                //Task.Run(async () => await AvatarService.UpdatePlayerAvatarAsync(player, player.Avatar));

                players.Add(player);
                PlayerLoginToId.Add(player.login.ToLower(), count);
                PlayerUIDToId.Add(player.id, count);
            }
        }
        private void OnPlayerReceived(object sender, PlayerInfoMessage player) => Task.Run(() => HandlePlayerData(player));

        #endregion


        public PlayerInfoMessage GetPlayer(string login)
        {
            if (string.IsNullOrEmpty(login)) return null;

            if (PlayerLoginToId.TryGetValue(login.ToLower(), out var id))
            {
                return Players[id];
            }

            return null;
        }

        public bool TryGetPlayer(string login, out PlayerInfoMessage player)
        {
            player = GetPlayer(login);
            if (string.IsNullOrWhiteSpace(login)) return false;

            return player is not null;
        }

        public PlayerInfoMessage GetPlayer(int uid)
        {
            if (PlayerUIDToId.TryGetValue(uid, out var id))
            {
                return Players[id];
            }

            return null;
        }


        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null)
        {
            var enumerator = _Players.GetEnumerator();

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

        private bool IsClanMate(string clan) => Equals(Me.clan, clan);
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

        public bool TryGetPlayer(int id, out PlayerInfoMessage player)
        {
            player = GetPlayer(id);
            return player is not null;
        }

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
    }
}
