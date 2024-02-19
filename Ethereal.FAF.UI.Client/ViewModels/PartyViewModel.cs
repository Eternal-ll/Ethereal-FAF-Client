using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class PartyFaction : ObservableObject
    {
        [ObservableProperty]
        private Faction _Faction;
        [ObservableProperty]
        private bool _Selected;
    }
    public partial class CurrentPlayerFaction : PartyFaction
    {
        [ObservableProperty]
        private IRelayCommand _UpdateSelectionCommand;
    }
    public partial class PartyPlayerMember : ObservableObject
    {
        [ObservableProperty]
        private long _PlayerId;
        [ObservableProperty]
        private bool _IsOwner;
        [ObservableProperty]
        private PartyFaction[] _PartyFactions;
        [ObservableProperty]
        private Player _Player;
        [ObservableProperty]
        private IRelayCommand _KickPlayerCommand;
    }
    [Singleton]
    public partial class PartyViewModel : Base.ViewModel
    {
        private readonly IFafPartyService _fafPartyService;
        private readonly IFafPlayersService _fafPlayersService;
        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly ILogger _logger;
        public PartyViewModel(
            IFafPartyService fafPartyService,
            IFafPlayersService fafPlayersService,
            IFafLobbyEventsService fafLobbyEventsService,
            ILogger<PartyViewModel> logger)
        {
            PlayerFactions = fafPartyService
                .GetFactions()
                .Select(x => new CurrentPlayerFaction()
                {
                    Faction = x,
                    UpdateSelectionCommand = UpdatePartyFactionCommand
                })
                .ToArray();

            PartyMembers = new();
            _partyMembersViewSource = new()
            {
                Source = PartyMembers
            };
            _partyMembersViewSource.SortDescriptions
                .Add(new(nameof(PartyPlayerMember.IsOwner), ListSortDirection.Descending));

            fafPartyService.OnUpdate += FafPartyService_OnUpdate;
            fafPartyService.OnKick += FafPartyService_OnKick;
            fafPartyService.OnInvite += FafPartyService_OnInvite;
            fafLobbyEventsService.OnConnection += FafLobbyEventsService_OnConnection;

            _logger = logger;
            _fafPartyService = fafPartyService;
            _fafPlayersService = fafPlayersService;
            _fafLobbyEventsService = fafLobbyEventsService;
        }

        private void FafLobbyEventsService_OnConnection(object sender, bool e)
        {
            if (e)
            {
                UpdatePartyFaction(PlayerFactions[0]);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var faction in PlayerFactions)
                    {
                        faction.Selected = false;
                    }
                    PartyMembers.Clear();
                }, DispatcherPriority.Background);
            }
        }

        private void FafPartyService_OnInvite(object sender, long e)
        {

        }

        private void FafPartyService_OnKick(object sender, System.EventArgs e)
        {
            SetPartyFactions();
        }

        private void FafPartyService_OnUpdate(object sender, (long Owner, PartyMember[] Members) e)
        {
            var currentMembers = PartyMembers;
            var incomingMembersIds = e.Members.Select(x => x.PlayerId).ToArray();

            var leftMembers = currentMembers
                .Where(x => !incomingMembersIds.Contains(x.PlayerId))
                .ToArray();
            _logger.LogDebug(
                "Party: players left party [{0}]",
                string.Join(',', leftMembers.Select(x => x.PlayerId)));
            if (leftMembers.Length > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var left in leftMembers)
                    {
                        currentMembers.Remove(left);
                    }
                }, DispatcherPriority.Background);
            }
            var currentMembersIds = currentMembers.Select(x => x.PlayerId).ToArray();

            foreach (var member in e.Members)
            {
                var playerMember = currentMembers
                    .FirstOrDefault(x => x.PlayerId == member.PlayerId);
                if (playerMember != null)
                {
                    foreach (var partyFaction in PlayerFactions)
                    {
                        partyFaction.Selected = member.Factions.Contains(partyFaction.Faction);
                    }
                }
            }

            var joined = incomingMembersIds
                .Except(currentMembersIds)
                .ToArray();
            _logger.LogDebug(
                "Party: players joined party [{0}]",
                string.Join(',', joined));
            var newPartyMembers = e.Members
                .Where(x => joined.Contains(x.PlayerId))
                .Select(x => new PartyPlayerMember()
                {
                    IsOwner = x.PlayerId == e.Owner,
                    PlayerId = x.PlayerId,
                    PartyFactions = PlayerFactions
                     .Select(f => new PartyFaction()
                     {
                         Faction = f.Faction,
                         Selected = x.Factions.Contains(f.Faction)
                     })
                     .ToArray(),
                    KickPlayerCommand = KickPlayerCommand,
                    Player = _fafPlayersService.TryGetPlayer(x.PlayerId, out var player) ? player : null
                })
                .ToArray();
            if (newPartyMembers.Length > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var member in newPartyMembers)
                    {
                        PartyMembers.Add(member);
                    }
                }, DispatcherPriority.Background);
            }
        }
        private Faction[] GetSelectedFactions() => PlayerFactions
            .Where(x => x.Selected)
            .Select(x => x.Faction)
            .ToArray();
        private void SetPartyFactions()
        {
            var selectedFactions = GetSelectedFactions();
            _fafPartyService.SetPartyFactions(selectedFactions);
        }

        [ObservableProperty]
        private CurrentPlayerFaction[] _PlayerFactions;
        [ObservableProperty]
        private ObservableCollection<PartyPlayerMember> _PartyMembers;
        private CollectionViewSource _partyMembersViewSource;
        public ICollectionView PartyMembersView => _partyMembersViewSource.View;

        [RelayCommand]
        private void KickPlayer(PartyPlayerMember member) => _fafPartyService.KickFromParty(member.PlayerId);
        [RelayCommand]
        private void UpdatePartyFaction(CurrentPlayerFaction partyFaction)
        {
            partyFaction.Selected = !partyFaction.Selected;
            var selectedFactions = GetSelectedFactions();
            _fafPartyService.SetPartyFactions(selectedFactions);
            if (selectedFactions.Length == 0)
                partyFaction.Selected = true;
        }
        [RelayCommand]
        private void LeaveParty()
        {
            _fafPartyService.LeaveParty();
            SetPartyFactions();
        }
        [RelayCommand]
        private async Task InvitePlayer()
        {

        }
    }
}
