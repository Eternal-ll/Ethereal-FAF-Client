using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public enum MatchmakingState : int
    {
        Idle,
        Searching,
        MatchFound,
        Confirming,
        Faulted,
        Playing,
        Updating,
    }
    public sealed class MatchmakingViewModel : Base.ViewModel
    {
        private readonly NotificationService NotificationService;

        private LobbyClient LobbyClient;
        private PatchClient PatchClient;

        public PartyViewModel PartyViewModel { get; private set; }

        public MatchmakingViewModel(NotificationService notificationService, PartyViewModel partyViewModel, IServiceProvider serviceProvider)
        {
            JoinQueueCommand = new LambdaCommand(OnJoinQueueCommand, CanJoinQueueCommand);
            NotificationService = notificationService;
            var serverManager = serviceProvider.GetRequiredService<ServerManager>();
            Initialize(serverManager.GetLobbyClient(), serverManager.GetPatchClient(), partyViewModel);
        }

        public void Initialize(LobbyClient lobbyClient, PatchClient patchClient,  PartyViewModel partyViewModel)
        {
            LobbyClient = lobbyClient;
            PartyViewModel = partyViewModel;
            PatchClient = patchClient;

            lobbyClient.StateChanged += LobbyClient_StateChanged;
            lobbyClient.SearchInfoReceived += LobbyClient_SearchInfoReceived;
            lobbyClient.MatchMakingDataReceived += LobbyClient_MatchMakingDataReceived;
            lobbyClient.MatchConfirmation += LobbyClient_MatchConfirmation;
            lobbyClient.MatchCancelled += LobbyClient_MatchCancelled;
            lobbyClient.MatchFound += LobbyClient_MatchFound;
        }

        private void LobbyClient_StateChanged(object sender, LobbyState e)
        {
            if (e is not LobbyState.Authorized) return;
            Task.Run(async () =>
            {
                while (LobbyClient.State is LobbyState.Authorized)
                {
                    // update queue pop time
                    var queues = Queues;
                    if (queues is not null)
                        foreach (var queue in queues)
                        {
                            if (queue.queue_pop_time_delta >= 0)
                            {
                                queue.queue_pop_time_delta--;
                                //queue.OnPropertyChanged(nameof(queue.PopTimeSpan));
                            }
                        }
                    // update match confirmation expiration
                    OnPropertyChanged(nameof(MatchConfirmation));

                    await Task.Delay(1000);
                }
            });
        }

        private void LobbyClient_MatchFound(object sender, MatchFound e)
        {
            NotificationService.Notify("Matchmaking", "Match found");
            foreach (var queue in SearchStates.Keys)
            {
                SearchStates[queue] = false;
            }
            State = MatchmakingState.Idle;
        }

        private void LobbyClient_MatchCancelled(object sender, MatchCancelled e)
        {
            NotificationService.Notify("Matchmaking", "Match was cancelled");
        }

        private async void LobbyClient_MatchConfirmation(object sender, MatchConfirmation e)
        {
            State = MatchmakingState.Confirming; 
            MatchConfirmation = e;
            if (e.IsReady) return;
            var result = await NotificationService.ShowDialog("Match confirmation", "Press \"Confirm\" button if you are ready to join the match", "Confirm", "Ignore");
            if (!result) return;
            LobbyClient.ReadyToJoinMatch();
        }

        private void LobbyClient_SearchInfoReceived(object sender, SearchInfo e)
        {
            SearchStates[e.Queue] = e.State == QueueSearchState.Start;
            if (CurrentQueue is null) return;
            if (CurrentQueue.Type == e.Queue)
            {
                State = e.State is QueueSearchState.Start ? MatchmakingState.Searching : MatchmakingState.Idle;
            }
        }

        private void LobbyClient_MatchMakingDataReceived(object sender, MatchmakingData e)
        {
            foreach (var queue in e.Queues)
            {
                var old = Queues.FirstOrDefault(q => q.Type == queue.Type);
                if (old is not null)
                {
                    Queues.Remove(old);
                }
                Queues.Add(queue);
                if (queue.IsGood(RatingType))
                    CurrentQueue = queue;
                //old.queue_pop_time = queue.queue_pop_time;
                //old.queue_pop_time_delta = queue.queue_pop_time_delta;
                //old.PlayersCountInQueue = queue.PlayersCountInQueue;
            }
        }

        public ObservableDictionary<MatchmakingType, bool> SearchStates { get; set; } = new()
            {
                { MatchmakingType.ladder1v1, false },
                { MatchmakingType.tmm2v2, false },
                { MatchmakingType.tmm4v4_full_share, false },
            };
        public bool AnyActiveQueue => SearchStates.Any(s => s.Value);

        public ObservableCollection<QueueData> Queues = new();

        #region MatchConfirmation
        private MatchConfirmation _MatchConfirmation;
        public MatchConfirmation MatchConfirmation { get => _MatchConfirmation; set => Set(ref _MatchConfirmation, value); }
        #endregion

        #region CurrentQueue
        private QueueData _CurrentQueue;
        public QueueData CurrentQueue
        {
            get => _CurrentQueue;
            set => Set(ref _CurrentQueue, value);
        }
        #endregion

        #region RatingType
        private RatingType _RatingType;
        public RatingType RatingType
        {
            get => _RatingType;
            set
            {
                if (Set(ref _RatingType, value))
                {
                    OnPropertyChanged(nameof(InviteButtonVisibility));
                    if (value is RatingType.global)
                    {
                        // setup any search state
                        var active = SearchStates.FirstOrDefault(t => t.Value);
                        if (active.Value)
                        {

                        }
                        return;
                    }
                    var queue = Queues.FirstOrDefault(q => q.IsGood(value));
                    if (queue is null) return;
                    State = SearchStates[queue.Type] ? MatchmakingState.Searching : MatchmakingState.Idle;
                    CurrentQueue = queue;
                }
            }
        }
        #endregion


        public Visibility InviteButtonVisibility =>
            State is MatchmakingState.Idle &&
            PartyViewModel.CanInvitePlayer &&
            PartyViewModel.IsOwner &&
            (RatingType is not RatingType.global and not RatingType.ladder_1v1 || PartyViewModel.HasMembers)?
            Visibility.Visible :
            Visibility.Collapsed;

        public bool IsProgressRingIndeterminate => ProgressRingVisibility is Visibility.Visible;
        #region ProgressRingVisibility
        private Visibility _ProgressRingVisibility = Visibility.Collapsed;
        public Visibility ProgressRingVisibility
        {
            get => _ProgressRingVisibility;
            set
            {
                if (Set(ref _ProgressRingVisibility, value))
                {
                    OnPropertyChanged(nameof(IsProgressRingIndeterminate));
                }
            }
        }
        #endregion

        #region ProgressText
        private string _ProgressText = "Search";
        public string ProgressText
        {
            get => _ProgressText;
            set => Set(ref _ProgressText, value);
        }
        #endregion

        #region State
        private MatchmakingState _State;
        public MatchmakingState State
        {
            get => _State;
            set
            {
                if (BackgroundCancellationTokenSource is not null)
                {
                    value = MatchmakingState.Updating;
                }
                if (Set(ref _State, value))
                {
                    ProgressText = value switch
                    {
                        MatchmakingState.Idle => "Search",
                        MatchmakingState.Searching => "Searching...",
                        MatchmakingState.Confirming => "Confirming...",
                        MatchmakingState.Faulted => "Faulted",
                        MatchmakingState.Playing => "Playing",
                        _=> value.ToString()
                    };
                    ProgressRingVisibility = value switch
                    {
                        MatchmakingState.Idle or
                        MatchmakingState.Faulted or
                        MatchmakingState.Playing => Visibility.Collapsed,
                        MatchmakingState.Searching or
                        MatchmakingState.Updating or
                        MatchmakingState.Confirming => Visibility.Visible,
                    };
                    OnPropertyChanged(nameof(InviteButtonVisibility));
                }
            }
        }
        #endregion

        private Task UpdateTask;
        public void SetupUpdateTask()
        {
            if (UpdateTask is not null) return;
            UpdateTask = Task.Run(() =>
            {

            });
        }
        private CancellationTokenSource CancellationTokenSource;

        public void LeaveFromAllQueues()
        {
            foreach (var key in SearchStates.Keys)
            {
                SearchStates[key] = false;
                LobbyClient.UpdateQueue(key, QueueSearchState.Stop);
            }
            NotificationService.Notify("Matchmaking", "You left from all matchmaking queues");
        }

        public bool CanSearch =>
            RatingType is RatingType.global ||
            (RatingType is RatingType.ladder_1v1 && PartyViewModel.Members.Count == 1) ||
            (RatingType is RatingType.tmm_2v2 && PartyViewModel.Members.Count <= 2) ||
            (RatingType is RatingType.tmm_4v4_full_share && PartyViewModel.Members.Count <= 4);

        #region JoinQueueCommand
        public ICommand JoinQueueCommand { get; }
        private bool CanJoinQueueCommand(object arg) => 
            (State is MatchmakingState.Idle && CanSearch) ||
            State is MatchmakingState.Searching;
        CancellationTokenSource BackgroundCancellationTokenSource;
        private async void OnJoinQueueCommand(object arg)
        {
            if (BackgroundCancellationTokenSource is not null)
            {
                BackgroundCancellationTokenSource.Cancel();
                State = MatchmakingState.Idle;
                return;
            }

            var queue = CurrentQueue is not null ? CurrentQueue.Type : RatingType switch
            {
                RatingType.ladder_1v1 => MatchmakingType.ladder1v1,
                RatingType.tmm_4v4_full_share => MatchmakingType.tmm4v4_full_share,
                RatingType.tmm_2v2 => MatchmakingType.tmm2v2,
            };
            var startSearch = !SearchStates[queue];

            if (startSearch && !SearchStates.Any(s => s.Value))
            {
                // first attempt to search, we can start patch confirmation and etc
                State = MatchmakingState.Updating;
                BackgroundCancellationTokenSource = new();
                ProgressRingVisibility = Visibility.Visible;
                ProgressText = "Confirming patch...";
                var progress = new Progress<string>((d) =>
                ProgressText = d.Length > 30 ? d.Truncate(30) : d);
                var cancel = false;
                //await PatchClient.ConfirmPatchAsync(
                //    mod: FeaturedMod.FAF,
                //    version: 0,
                //    forceCheck: false,
                //    BackgroundCancellationTokenSource.Token,
                //    progress)
                //    .ContinueWith(t =>
                //    {
                //        if (t.IsFaulted || t.IsCanceled) cancel = true;
                //    });
                if (cancel) return;


                BackgroundCancellationTokenSource = null;
            }






            SearchStates[queue] = startSearch;
            LobbyClient.UpdateQueue(queue, startSearch ? QueueSearchState.Start : QueueSearchState.Stop);
            if (!startSearch) return;
            if (UpdateTask is null)
            {
                SetupUpdateTask();
            }


            return;
            //State = MatchmakingState.Searching;
            //CancellationTokenSource = new();
            //await Task.Delay(2000);
            //if (CancellationTokenSource.IsCancellationRequested) return;
            //State = MatchmakingState.Confirming;
            //var dialog = DialogService.GetDialogControl();
            //dialog.ButtonLeftName = "Join";
            //dialog.ButtonRightName = "Cancel";
            //var result = await dialog.ShowAndWaitAsync("Match confirmation", "Confirm to join to match", true);
            //if (result is Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Right)
            //{
            //    State = MatchmakingState.Idle;
            //}
            //else if (result is Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Left)
            //{
            //    State = MatchmakingState.Playing;
            //}
        }
        #endregion
        #region LeaveQueueCommand
        private ICommand _LeaveQueueCommand;
        public ICommand LeaveQueueCommand => _LeaveQueueCommand ??= new LambdaCommand(OnLeaveQueueCommand, CanLeaveQueueCommand);

        private bool CanLeaveQueueCommand(object arg) => State is not MatchmakingState.Idle;
        private void OnLeaveQueueCommand(object arg)
        {
            //State = MatchmakingState.Idle;
            //CancellationTokenSource.Cancel();
        }
        #endregion
    }
}
