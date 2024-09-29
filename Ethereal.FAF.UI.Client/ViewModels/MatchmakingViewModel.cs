using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class QueueViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _TechnicalName;
        [ObservableProperty]
        private bool _Searching;
        [ObservableProperty]
        private int _NumPlayers;
        [ObservableProperty]
        private int _TeamSize;
        [ObservableProperty]
        private double _PopTimeDelta;
        [ObservableProperty]
        private TimeSpan _PopTimeSpan;
        [ObservableProperty]
        private DateTimeOffset _PopTime;
        [ObservableProperty]
        private ICommand _UpdateStateCommand;
        [ObservableProperty]
        private ICommand _ShowMapPoolCommand;
    }
    [Singleton]
    public partial class MatchmakingViewModel : Base.ViewModel
    {
        private readonly IFafMatchmakerService _matchmakerService;
        private readonly IFafLobbyEventsService _lobbyEventsService;
        private readonly IPatchClient _patchClient;
        private readonly Timer _timer;
        private readonly Dictionary<string, QueueSearchState> _queueStates = new();
        public MatchmakingViewModel(
            PartyViewModel partyViewModel,
            IFafMatchmakerService matchmakerService,
            IFafLobbyEventsService lobbyEventsService,
            IPatchClient patchClient)
        {
            Queues = new();
            _queuesCollectionViewSource = new()
            {
                Source = Queues
            };
            _queuesCollectionViewSource.SortDescriptions.Add(new SortDescription(nameof(QueueViewModel.TeamSize), ListSortDirection.Ascending));
            _timer = new(new(x =>
            {
                var queues = Queues;
                foreach (var queue in queues)
                {
                    queue.PopTimeDelta -= 1;
                    queue.PopTimeSpan = TimeSpan.FromSeconds(queue.PopTimeDelta);
                }
            }), null, 500, 1000);
            matchmakerService.OnMatchCancelled += MatchmakerService_OnMatchCancelled;
            matchmakerService.OnMatchFound += MatchmakerService_OnMatchFound;
            matchmakerService.OnMatchConfirmation += MatchmakerService_OnMatchConfirmation;
            matchmakerService.OnQueues += MatchmakerService_OnQueues;
            matchmakerService.OnSearch += MatchmakerService_OnSearch;
            lobbyEventsService.OnConnection += LobbyEventsService_OnConnection;
            _PartyViewModel = partyViewModel;
            _matchmakerService = matchmakerService;
            _lobbyEventsService = lobbyEventsService;
            _patchClient = patchClient;
        }

        [ObservableProperty]
        private PartyViewModel _PartyViewModel;
        [ObservableProperty]
        private ObservableCollection<QueueViewModel> _Queues;
        private CollectionViewSource _queuesCollectionViewSource;
        public ICollectionView QueuesView => _queuesCollectionViewSource.View;

        private void LobbyEventsService_OnConnection(object sender, bool e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e) RefreshQueues();
                else Queues.Clear();
            });
        }

        private void MatchmakerService_OnSearch(object sender, SearchInfo e)
        {
            if (!_queueStates.ContainsKey(e.Queue))
            {
                _queueStates.Add(e.Queue, e.State);
            }
            else
            {
                _queueStates[e.Queue] = e.State;
            }
            var queue = Queues.FirstOrDefault(x => x.TechnicalName == e.Queue);
            if (queue == null)
            {
                if (e.State == QueueSearchState.Start)
                {
                    _matchmakerService.UpdateQueueState(e.Queue, QueueSearchState.Stop);
                }
            }
            else
            {
                queue.Searching = e.State == QueueSearchState.Start;
            }
        }

        private void MatchmakerService_OnQueues(object sender, QueueData[] e)
        {
            var queues = Queues;
            foreach (var queue in e)
            {
                var queueViewModel = queues.FirstOrDefault(x => x.TechnicalName == queue.QueueName);
                if (queueViewModel == null)
                {
                    queueViewModel = new()
                    {
                        TechnicalName = queue.QueueName,
                        PopTime = queue.QueuePopTime,
                        PopTimeSpan = TimeSpan.FromSeconds(queue.QueuePopTimeDelta),
                        UpdateStateCommand = UpdateQueueStateCommand,
                        TeamSize = queue.TeamSize,
                        NumPlayers = queue.NumPlayers,
                        Searching = _queueStates.TryGetValue(queue.QueueName, out var state) && state == QueueSearchState.Start,
                        PopTimeDelta = queue.QueuePopTimeDelta
                    };
                    Application.Current.Dispatcher.Invoke(
                        () => Queues.Add(queueViewModel),
                        DispatcherPriority.Background);
                }
                else
                {
                    queueViewModel.PopTimeSpan = TimeSpan.FromSeconds(queue.QueuePopTimeDelta);
                    queueViewModel.TeamSize = queue.TeamSize;
                    queueViewModel.NumPlayers = queue.NumPlayers;
                    queueViewModel.PopTimeDelta = queue.QueuePopTimeDelta;
                }
            }
        }

        private void MatchmakerService_OnMatchConfirmation(object sender, MatchConfirmation e)
        {
            // TODO user dialog popup
            if (!e.IsReady) _matchmakerService.MatchReady();
        }

        private void MatchmakerService_OnMatchFound(object sender, MatchFound e)
        {

        }

        private void MatchmakerService_OnMatchCancelled(object sender, MatchCancelled e)
        {

        }
        [RelayCommand]
        private void RefreshQueues()
        {
            Queues.Clear();
            _matchmakerService.RefreshQueues();
        }
        [RelayCommand]
        private void UpdateQueueState(QueueViewModel queue)
        {
            var state = queue.Searching ?
                QueueSearchState.Stop : QueueSearchState.Start;
            _matchmakerService.UpdateQueueState(queue.TechnicalName, state);
        }
    }
}
