using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace beta.ViewModels
{
    public class MatchMakerViewModel : ViewModel
    {
        private readonly ISessionService SessionService;
        private readonly IQueueService QueueService;
        private readonly ILogger Logger;

        private DispatcherTimer DispatcherTimer;
        public MatchMakerViewModel(ISessionService sessionService, IQueueService queueService, ILogger<MatchMakerViewModel> logger)
        {
            SessionService = sessionService;
            QueueService = queueService;
            Logger = logger;

            SessionService.MatchMakerDataReceived += SessionService_MatchMakerDataReceived;

            GamesViewModel = new();

            DispatcherTimer = new(DispatcherPriority.Background, App.Current.Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            DispatcherTimer.Tick += DispatcherTimer_Tick;
            DispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            var queues = Queues;
            foreach (var queue in queues)
            {
                if (queue.queue_pop_time_delta > 0)
                {
                    queue.queue_pop_time_delta--;
                }
            }
        }

        private void SessionService_MatchMakerDataReceived(object sender, MatchMakerData e)
        {
            for (int i = 0; i < e.Queues.Length; i++)
            {
                var incomeQueue = e.Queues[i];
                for (int j = 0; j < Queues.Length; j++)
                {
                    var queue = Queues[j];
                    if (queue.Type == incomeQueue.Type)
                    {
                        queue.queue_pop_time = incomeQueue.queue_pop_time;
                        queue.queue_pop_time_delta = incomeQueue.queue_pop_time_delta;
                        queue.CountInQueue = incomeQueue.CountInQueue;
                        break;
                    }
                }
            }
        }

        public QueueData[] Queues { get; set; } = new QueueData[]
        {
            new QueueData()
            {
                Type = Models.Server.Enums.MatchMakerType.ladder1v1
            },
            new QueueData()
            {
                Type = Models.Server.Enums.MatchMakerType.tmm2v2
            },
            new QueueData()
            {
                Type = Models.Server.Enums.MatchMakerType.tmm4v4_full_share
            },
            new QueueData()
            {
                Type = Models.Server.Enums.MatchMakerType.tmm4v4_share_until_death
            },
        };

        #region CurrentQueue
        private QueueData _CurrentQueue;
        public QueueData CurrentQueue
        {
            get => _CurrentQueue;
            set => Set(ref _CurrentQueue, value);
        }
        #endregion

        #region GamesViewModel
        private MatchMakerGamesViewModel _GamesViewModel;
        public MatchMakerGamesViewModel GamesViewModel
        {
            get => _GamesViewModel;
            set => Set(ref _GamesViewModel, value);
        }
        #endregion

        #region JoinQueueCommand
        private ICommand _JoinQueueCommand;
        public ICommand JoinQueueCommand => _JoinQueueCommand ??= new LambdaCommand(OnJoinQueueCommand, CanJoinQueueCommand);
        private bool CanJoinQueueCommand(object parameter) => true;
        private void OnJoinQueueCommand(object parameter)
        {
            QueueService.SignUpQueue(CurrentQueue.Type);
        }
        #endregion

        private ICommand _LeaveQueueCommand;
        private ICommand LeaveQueueCommand => _LeaveQueueCommand ??= new LambdaCommand(OnLeaveQueueCommand);
        private void OnLeaveQueueCommand(object parameter)
        {
            QueueService.SignUpQueue(CurrentQueue.Type);
        }
    }
}
