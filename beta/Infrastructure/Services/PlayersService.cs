using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services
{
    public class PlayersService : ViewModel, IPlayersService
    {

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

        private int[] FriendsIds { get; set; } = Array.Empty<int>();
        private int[] FoesIds { get; set; } = Array.Empty<int>();

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

            sessionService.NewPlayer += OnNewPlayer;
            sessionService.SocialInfo += OnNewSocialInfo;
            sessionService.WelcomeInfo += OnWelcomeInfo;

            System.Windows.Application.Current.Exit += (s, e) =>
            {
                var players = Players;
                for (int i = 0; i < players.Count; i++)
                {
                    var player = players[i];
                    if (player.Note.Text.Trim().Length > 0)
                    {
                        NoteService.Set(player.login, player.Note.Text);
                    }
                }
                NoteService.Save();
            };
        }


        #endregion

        private PlayerInfoMessage Me;

        #region Event listeners
        private void OnWelcomeInfo(object sender, EventArgs<WelcomeMessage> e)
        {
            Me = e.Arg.me;
            Me.RelationShip = PlayerRelationShip.Me;
        }
        private void OnNewSocialInfo(object sender, EventArgs<SocialMessage> e)
        {
            FriendsIds = e.Arg.friends;
            FoesIds = e.Arg.foes;

            var friendsIds = e.Arg.friends;
            var foesIds = e.Arg.foes;

            // TODO REWRITE?
            if (friendsIds is not null && friendsIds.Length > 0)
                for (int i = 0; i < friendsIds.Length; i++)
                    if (PlayerUIDToId.TryGetValue(friendsIds[i], out var id))
                    {
                        var player = Players[id];
                        if (friendsIds[i] == player.id)
                        {
                            player.RelationShip = PlayerRelationShip.Friend;
                            return;
                        }
                    }

            if (foesIds is not null && foesIds.Length > 0)
                for (int i = 0; i < foesIds.Length; i++)
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
        private void OnNewPlayer(object sender, EventArgs<PlayerInfoMessage> e)
        {
            var player = e.Arg;
            var players = _Players;

            #region Add note about player

            if (NoteService.TryGet(player.login, out var note))
            {
                player.Note = new(note);
            }
            else
            {
                player.Note = new();
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

                if (!Equals(matchedPlayer.avatar, player.avatar) && player.avatar != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    matchedPlayer.Avatar = AvatarService.GetAvatar(player.avatar.url),
                    System.Windows.Threading.DispatcherPriority.Background);
                }

                matchedPlayer.Update(player);
            }
            else
            {
                int count = players.Count;

                // Check for avatar
                if (player.avatar is not null)
                {
                    // TODO FIX ME Thread access error. BitmapImage should be created in UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() => player.Avatar = AvatarService.GetAvatar(player.avatar.url),
                    System.Windows.Threading.DispatcherPriority.Background);
                }

                players.Add(player);
                PlayerLoginToId.Add(player.login.ToLower(), count);
                PlayerUIDToId.Add(player.id, count);
            }
        }

        #endregion


        public PlayerInfoMessage GetPlayer(string login)
        {
            login = login.ToLower();
            if (login.Length <= 0 || login.Trim().Length <= 0) return null;

            if (PlayerLoginToId.TryGetValue(login, out var id))
            {
                return Players[id];
            }

            return null;
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
            if (friends.Length == 0) return false;

            for (int i = 0; i < friends.Length; i++)
            {
                if (friends[i] == id) return true;
            }
            return false;
        }
        public bool IsFriend(PlayerInfoMessage player) => IsFriend(player.id);

        private bool IsFoe(int id)
        {
            var foes = FoesIds;
            if (foes.Length == 0) return false;

            for (int i = 0; i < foes.Length; i++)
            {
                if (foes[i] == id) return true;
            }
            return false;
        }
        public bool IsFoe(PlayerInfoMessage player) => IsFoe(player.id);
    }
}
