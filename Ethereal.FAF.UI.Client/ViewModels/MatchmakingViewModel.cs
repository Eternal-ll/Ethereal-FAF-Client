using ABI.System;
using AsyncAwaitBestPractices.MVVM;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public enum MatchmakingState : int
    {
        Idle,
        Searching,
        MatchFound,
        Confirming,
        Faulted,
        Playing
    }
    public class MatchmakingViewModel : Base.ViewModel
    {
        private readonly LobbyClient LobbyClient;
        private readonly DialogService DialogService;

        public MatchmakingViewModel(LobbyClient lobbyClient, DialogService dialogService)
        {
            LobbyClient = lobbyClient;


            lobbyClient.MatchMakingDataReceived += LobbyClient_MatchMakingDataReceived;
            DialogService = dialogService;

            Task.Run(async () =>
            {
                while (true)
                {
                    var queues = Queues;
                    if (queues is not null)
                    foreach (var queue in queues)
                    {
                        if (queue.queue_pop_time_delta >= 0)
                        {
                            queue.queue_pop_time_delta--;
                            queue.OnPropertyChanged(nameof(queue.PopTimeSpan));
                        }
                    }
                    await Task.Delay(1000);
                }
            });
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
                if (queue.IsGood(RatingType)) CurrentQueue = queue;
                //old.queue_pop_time = queue.queue_pop_time;
                //old.queue_pop_time_delta = queue.queue_pop_time_delta;
                //old.PlayersCountInQueue = queue.PlayersCountInQueue;
            }
        }

        public ObservableCollection<QueueData> Queues = new();

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
                    var queue = Queues.FirstOrDefault(q => q.IsGood(value));
                    CurrentQueue = queue;
                }
            }
        }
        #endregion

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
                if (Set(ref _State, value))
                {
                    ProgressText = value switch
                    {
                        MatchmakingState.Idle => "Search",
                        MatchmakingState.Searching => "Searching...",
                        MatchmakingState.Confirming => "Confirming...",
                        MatchmakingState.Faulted => "Faulted",
                        MatchmakingState.Playing => "Playing",
                    };
                    ProgressRingVisibility = value switch
                    {
                        MatchmakingState.Idle or
                        MatchmakingState.Faulted or
                        MatchmakingState.Playing => Visibility.Collapsed,
                        MatchmakingState.Searching or
                        MatchmakingState.Confirming => Visibility.Visible
                    };
                }
            }
        }
        #endregion

        private CancellationTokenSource CancellationTokenSource;

        #region JoinQueueCommand
        private ICommand _JoinQueueCommand;
        public ICommand JoinQueueCommand => _JoinQueueCommand ??= new AsyncCommand<object>(OnJoinQueueCommand, CanJoinQueueCommand);

        private bool CanJoinQueueCommand(object arg) => State is MatchmakingState.Idle;
        private async Task OnJoinQueueCommand(object arg)
        {
            State = MatchmakingState.Searching;
            CancellationTokenSource = new();
            await Task.Delay(2000);
            if (CancellationTokenSource.IsCancellationRequested) return;
            State = MatchmakingState.Confirming;
            var dialog = DialogService.GetDialogControl();
            dialog.ButtonLeftName = "Join";
            dialog.ButtonRightName = "Cancel";
            var result = await dialog.ShowAndWaitAsync("Match confirmation", "Confirm to join to match", true);
            if (result is Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Right)
            {
                State = MatchmakingState.Idle;
            }
            else if (result is Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Left)
            {
                State = MatchmakingState.Playing;
            }
        }
        #endregion
        #region JoinQueueCommand
        private ICommand _LeaveQueueCommand;
        public ICommand LeaveQueueCommand => _LeaveQueueCommand ??= new LambdaCommand(OnLeaveQueueCommand, CanLeaveQueueCommand);

        private bool CanLeaveQueueCommand(object arg) => State is not MatchmakingState.Idle;
        private void OnLeaveQueueCommand(object arg)
        {
            State = MatchmakingState.Idle;
            CancellationTokenSource.Cancel();
        }
        #endregion
    }
}
