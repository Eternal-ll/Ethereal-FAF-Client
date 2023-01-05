using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Views;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class PartyPlayer
    {
        public PartyPlayer()
        {

        }
        public PartyPlayer(Player player, Faction[] factions, bool isOwner)
        {
            Player = player;
            IsOwner = isOwner;
            foreach (var faction in factions)
            {
                Factions[faction] = true;
            }
        }

        public Player Player { get; set; }
        public Dictionary<Faction, bool> Factions { get; set; } = new()
            {
                { Faction.UEF, false },
                { Faction.CYBRAN, false },
                { Faction.AEON, false },
                { Faction.SERAPHIM, false },
            };
        public Faction[] SelectedFactions => Factions.Where(t => t.Value).Select(t => t.Key).ToArray();
        public bool IsOwner { get; set; }
    }
    public sealed class PartyViewModel : Base.ViewModel
    {
        private object _Status;
        public object Status
        {
            get => _Status;
            set
            {
                if (Set(ref _Status, value))
                {
                    // event ?
                    // notification ?
                }
            }
        }

        private readonly LobbyClient LobbyClient;
        private readonly PlayersViewModel PlayersVM;
        private readonly DialogService DialogService;
        private readonly NotificationService NotificationService;
        private readonly IServiceProvider ServiceProvider;
        private readonly INavigationService INavigationService;
        private readonly ILogger Logger;

        public PartyViewModel(PlayersViewModel playersVM, ILogger<PartyViewModel> logger, DialogService dialogService, IServiceProvider serviceProvider, NotificationService notificationService, INavigationService iNavigationService)
        {
            //LobbyClient = lobbyClient;
            PlayersVM = playersVM;
            DialogService = dialogService;
            ServiceProvider = serviceProvider;
            NotificationService = notificationService;
            INavigationService = iNavigationService;
            Logger = logger;

            //lobbyClient.KickedFromParty += LobbyClient_KickedFromParty;
            //lobbyClient.PartyInvite += LobbyClient_PartyInvite;
            //lobbyClient.PartyUpdated += LobbyClient_PartyUpdated;
            //lobbyClient.Authorized += LobbyClient_Authorized;

            Factions = new()
            {
                { Faction.UEF, true },
                { Faction.CYBRAN, true },
                { Faction.AEON, true },
                { Faction.SERAPHIM, true },
            };
            Members = new();
        }

        private void LobbyClient_Authorized(object sender, bool e)
        {
            if (!e) return;
            LobbyClient.SetPartyFactions(SelectedFactions);
        }

        private long _OwnerId;
        public long OwnerId
        {
            get => _OwnerId;
            set
            {
                if (Set(ref _OwnerId, value))
                {
                    IsOwner = PlayersVM.Selfs.Any(s=>s.Id == value);
                }
            }
        }
        private bool _IsOwner;
        public bool IsOwner
        {
            get => _IsOwner;
            set => Set(ref _IsOwner, value);
        }
        public bool CanInvitePlayer => Members.Count < 4;
        public bool HasMembers => Members.Count > 1;

        //#region Owner
        //private Player _Owner;
        //public Player Owner
        //{
        //    get => _Owner;
        //    set => Set(ref _Owner, value);
        //}
        //#endregion

        public ConcurrentObservableCollection<PartyPlayer> Members { get; }
        public IReadOnlyObservableCollection<PartyPlayer> MembersObservable => Members.AsObservable;
        public ObservableDictionary<Faction, bool> Factions { get; set; }
        public Faction[] SelectedFactions => Factions.Where(t => t.Value).Select(t => t.Key).ToArray();

        private void LobbyClient_PartyUpdated(object sender, PartyUpdate e)
        {
            foreach (var member in e.Members)
            {
                // ADD server manager
                if (PlayersVM.TryGetPlayer(member.PlayerId, null, out var player))
                {
                    var playerMember = Members.FirstOrDefault(m => m.Player.Id == member.PlayerId);
                    var partyPlayer = new PartyPlayer(player, member.Factions, member.PlayerId == e.OwnerId);
                    if (playerMember is not null)
                    {
                        Members[Members.IndexOf(playerMember)] = partyPlayer;
                        if (playerMember.SelectedFactions.Length == member.Factions.Length) continue;
                        Logger.LogTrace("[Party] Player [{id}] updated factions from [{old}] to [{new}]",
                            member.PlayerId, playerMember.SelectedFactions, member.Factions);
                        NotificationService.Notify("Party", $"Player [{member.PlayerId}] updated factions to [{string.Join(',', partyPlayer.SelectedFactions)}]");
                        continue;
                    }
                    Members.Add(new PartyPlayer(player, member.Factions, member.PlayerId == e.OwnerId));
                    Logger.LogTrace("[Party] Player [{id}] joined to party", member.PlayerId);
                    // TODO Notify by server
                    if (PlayersVM.Selfs.Any(s => s.Id == member.PlayerId)) continue;
                    NotificationService.Notify("Party", $"Player [{member.PlayerId}] joined to party with these factions [{string.Join(',', partyPlayer.SelectedFactions)}]");
                }
            }
            var left = Members.Where(m => !e.Members.Any(n => n.PlayerId == m.Player.Id));
            foreach (var member in left)
            {
                Members.Remove(member);
                Logger.LogTrace("[Party] Player [{id}] left from party", member.Player.Id);
                NotificationService.Notify("Party", string.Format("Player [{0}] left from party", member.Player.Id));
            }

            if (OwnerId != e.OwnerId)
            {
                Logger.LogTrace("[Party] Owner [{old}] updated to [{}]", OwnerId, e.OwnerId);
                OwnerId = e.OwnerId;
            }

            Logger.LogTrace("[Party] Updated");
        }

        private async void LobbyClient_PartyInvite(object s, PartyInvite e)
        {
            Logger.LogTrace("[Party] Party invite from player [{id}]", e.SenderId);
            var dialog = DialogService.GetDialogControl();
            // TODO add server maanger
            if (PlayersVM.TryGetPlayer(e.SenderId, null, out var sender))
            {
                var accepted = await NotificationService.ShowDialog("Party", $"Player {sender.Login} invites you to party", "Accept", "Ignore");
                if (accepted)
                {
                    LobbyClient.AcceptPartyInviteFromPlayer(e.SenderId);
                }
            }
        }

        private async void LobbyClient_KickedFromParty(object sender, System.EventArgs e)
        {
            //NotificationService.Notify("Notification", "You were kicked from party");
            Logger.LogTrace("[Party] Kicked from party of player [{id}]", OwnerId);
            LobbyClient.SetPartyFactions(SelectedFactions);
        }

        #region BackCommand
        private ICommand _BackCommand;
        public ICommand BackCommand => _BackCommand ??= new LambdaCommand(OnBackCommand);
        private void OnBackCommand(object obj)
        {
            var playersView = ServiceProvider.GetService<PlayersView>();
            playersView.DisableInvitePlayerCommand();
            var frame = INavigationService.GetFrame();
            INavigationService.Navigate(0);
        }
        #endregion

        #region ShowInviteBoxCommand
        private ICommand _ShowInviteBoxCommand;
        public ICommand ShowInviteBoxCommand => _ShowInviteBoxCommand ??= new LambdaCommand(OnShowInviteBoxCommand, CanShowInviteBoxCommand);
        private bool CanShowInviteBoxCommand(object arg) => true;
        private void OnShowInviteBoxCommand(object obj)
        {
            var playersView = ServiceProvider.GetService<PlayersView>();
            playersView.EnableInvitePlayerCommand(InvitePlayerCommand, BackCommand);
            INavigationService.Navigate(2);
        }
        #endregion

        #region InvitePlayerCommand
        private ICommand _InvitePlayerCommand;
        public ICommand InvitePlayerCommand => _InvitePlayerCommand ??= new LambdaCommand(OnInvitePlayerCommand, CanInvitePlayerCommand);
        private bool CanInvitePlayerCommand(object arg) => true;
        private void OnInvitePlayerCommand(object obj)
        {
            if (obj is not long id) return;
            NotificationService.Notify("Invite", $"Player with id {id} invited.");
            LobbyClient.InvitePlayerToParty(id);
        }
        #endregion

        #region UpdateFactionCommand
        private ICommand _UpdateFactionCommand;
        public ICommand UpdateFactionCommand => _UpdateFactionCommand ??= new LambdaCommand(OnUpdateFactionCommand, CanUpdateFactionCommand);
        private bool CanUpdateFactionCommand(object arg) => true;
        private void OnUpdateFactionCommand(object obj)
        {
            if (obj is not Faction faction) return;
            if (SelectedFactions.Length == 1 && SelectedFactions[0] == faction) return;
            var item = Factions[faction];
            Factions[faction] = !item;
            LobbyClient.SetPartyFactions(SelectedFactions);
        }
        #endregion

        #region KickPlayerCommand
        private ICommand _KickPlayerCommand;
        public ICommand KickPlayerCommand => _KickPlayerCommand ??= new LambdaCommand(OnKickPlayerCommand, CanKickPlayerCommand);
        private bool CanKickPlayerCommand(object arg) => true;
        private void OnKickPlayerCommand(object obj)
        {
            if (obj is not long id) return;
            LobbyClient.KickPlayerFromParty(id);
        } 
        #endregion
    }
}
