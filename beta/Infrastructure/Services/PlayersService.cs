using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services
{
    public enum ComparisonMethod
    {
        STARTS_WITH = 0,
        CONTAINS = 1
    }
    public class PlayersService : ViewModel, IPlayersService
    {

        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IAvatarService AvatarService;

        #endregion

        #region Players

        public readonly ObservableCollection<PlayerInfoMessage> _Players = new();
        public ObservableCollection<PlayerInfoMessage> Players => _Players;

        private readonly Dictionary<int, int> PlayerUIDToId = new();
        private readonly Dictionary<string, int> PlayerLoginToId = new();

        #endregion

        private int[] FriendsIds { get; set; }
        private int[] FoesIds { get; set; }

        #endregion

        #region Ctor

        public PlayersService(
            ISessionService sessionService,
            IAvatarService avatarService)
        {
            SessionService = sessionService;
            AvatarService = avatarService;

            sessionService.NewPlayer += OnNewPlayer;
            sessionService.SocialInfo += OnNewSocialInfo;
        }

        #endregion

        #region Event listeners
        private void OnNewSocialInfo(object sender, EventArgs<SocialMessage> e)
        {
            FriendsIds = e.Arg.friends;
            FoesIds = e.Arg.foes;

            var friendsIds = e.Arg.friends;
            var foesIds = e.Arg.foes;

            // TODO REWRITE?
            if (friendsIds != null && friendsIds.Length > 0)
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

            if (foesIds != null && foesIds.Length > 0)
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

            // TODO REWRITE?
            #region Matching friends & foes

            var friendsIds = FriendsIds;
            var foesIds = FoesIds;

            // TODO REWRITE?
            if (friendsIds != null && friendsIds.Length > 0)
                for (int i = 0; i < friendsIds.Length; i++)
                    if (friendsIds[i] == player.id)
                    {
                        player.RelationShip = PlayerRelationShip.Friend;
                        break;
                    }

            if (foesIds != null && foesIds.Length > 0)
                for (int i = 0; i < foesIds.Length; i++)
                    if (foesIds[i] == player.id)
                    {
                        player.RelationShip = PlayerRelationShip.Foe;
                        break;
                    }

            #endregion

            if (PlayerUIDToId.TryGetValue(player.id, out var id))
            {
                var matchedPlayer = players[id];

                // If avatar is changed
                if (matchedPlayer.avatar != null)
                {
                    if (player.avatar != null)
                    {
                        if (player.avatar.url.Segments[^1] != matchedPlayer.avatar.url.Segments[^1])
                            // TODO FIX ME Thread access error. BitmapImage should be created in UI thread
                            System.Windows.Application.Current.Dispatcher.Invoke(() => matchedPlayer.Avatar = AvatarService.GetAvatar(player.avatar.url),
                            System.Windows.Threading.DispatcherPriority.Background);
                    }
                    else player.Avatar = null;
                }

                matchedPlayer.Update(player);
            }
            else
            {
                int count = players.Count;

                // Check for avatar
                if (player.avatar != null)
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
    }
}
