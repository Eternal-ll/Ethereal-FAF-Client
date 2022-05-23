using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace beta.ViewModels
{
    internal class MatchMakerViewModel : ViewModel
    {
        private readonly ISessionService SessionService;
        private readonly ILogger Logger;
        public MatchMakerViewModel()
        {
            SessionService = App.Services.GetService<ISessionService>();
            Logger = App.Services.GetService<ILogger<MatchMakerViewModel>>();

            SessionService.MatchMakerDataReceived += SessionService_MatchMakerDataReceived;

            GamesViewModel = new();
        }

        private void SessionService_MatchMakerDataReceived(object sender, MatchMakerData e)
        {
            for (int i = 0; i < e.Queues.Length; i++)
            {
                for (int j = 0; j < Queues.Length; j++)
                {
                    var queue = Queues[j];
                    if (queue.Type == e.Queues[i].Type)
                    {
                        Queues[j] = e.Queues[i];
                        OnPropertyChanged(nameof(Queues));
                        break;
                    }
                }
            }
        }

        public QueueDataModel[] Queues { get; set; } = new QueueDataModel[]
        {
            new QueueDataModel()
            {
                Type = Models.Server.Enums.MatchMakerType.ladder1v1,
                CountInQueue = new Random().Next(0,100)
            },
            new QueueDataModel()
            {
                Type = Models.Server.Enums.MatchMakerType.tmm2v2,
                CountInQueue = new Random().Next(0,100)
            },
            new QueueDataModel()
            {
                Type = Models.Server.Enums.MatchMakerType.tmm4v4_full_share,
                CountInQueue = new Random().Next(0,100)
            },
            new QueueDataModel()
            {
                Type = Models.Server.Enums.MatchMakerType.tmm4v4_share_until_death,
                CountInQueue = new Random().Next(0,100)
            },
        };

        #region CurrentQueue
        private QueueDataModel _CurrentQueue;
        public QueueDataModel CurrentQueue
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
    }
}
