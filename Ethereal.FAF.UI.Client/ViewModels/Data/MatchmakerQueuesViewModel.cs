using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using FAForever.Api.Client;
using FAForever.Api.Client.Queries;
using FAForever.Api.Client.Queries.Linq;
using SharpCompress;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.ViewModels.Data
{
    public class MatchmakerQueue : FAForever.Api.Domain.Entities.MatchmakerQueue;
    public class MatchmakerQueueMapPool : FAForever.Api.Domain.Entities.MatchmakerQueueMapPool;
    public class MapPool : FAForever.Api.Domain.Entities.MapPool;
    public class MapPoolAssignment : FAForever.Api.Domain.Entities.MapPoolAssignment;
    public class MapVersion : FAForever.Api.Domain.Entities.MapVersion;
    [Singleton]
    public partial class MatchmakerQueuesViewModel : Base.ViewModel
    {
        private readonly IFafApi _fafApi;

        public MatchmakerQueuesViewModel(IFafApi fafApi)
        {
            _fafApi = fafApi;
        }
        [ObservableProperty]
        private MatchmakerQueue[] _MatchmakerQueues;

        #region SelectedMatchmakerQueue
        private MatchmakerQueue _SelectedMatchmakerQueue;
        public MatchmakerQueue SelectedMatchmakerQueue
        {
            get => _SelectedMatchmakerQueue;
            set
            {
                if (SetProperty(ref _SelectedMatchmakerQueue, value))
                {
                    Task.Run(LoadSelectedQueueDataAsync).SafeFireAndForget();
                }
            }
        }

        [ObservableProperty]
        private MatchmakerQueueMapPool[] _MatchmakerQueueMapPools;

        #region SelectedMatchmakerQueueMapPool

        private MatchmakerQueueMapPool _SelectedMatchmakerQueueMapPool;
        public MatchmakerQueueMapPool SelectedMatchmakerQueueMapPool
        {
            get => _SelectedMatchmakerQueueMapPool;
            set
            {
                if (SetProperty(ref _SelectedMatchmakerQueueMapPool, value))
                {
                    Task.Run(LoadSelectedMatchmakerQueueMapPoolAsync).SafeFireAndForget();
                }
            }
        }
        [ObservableProperty]
        private MapPoolAssignment[] _MapPoolAssignments;
        #endregion
        #endregion

        public override async Task OnLoadedAsync()
        {
            MatchmakerQueues = await _fafApi.MatchmakerQueues
                .FetchDataAsync()
                .ContinueWith(x => x.IsFaulted ? null : x.Result.Data
                    .Select(x => new MatchmakerQueue()
                    {
                        Id = x.Id,
                        Attributes = x.Attributes
                    })
                    .ToArray(), TaskScheduler.Default);
        }
        private async Task LoadSelectedQueueDataAsync()
        {
            var selectedMatchmakerQueueId = SelectedMatchmakerQueue?.Id;
            MatchmakerQueueMapPools = null;
            SelectedMatchmakerQueueMapPool = null;
            if (!selectedMatchmakerQueueId.HasValue) return;
            MatchmakerQueueMapPools = await _fafApi.MatchmakerQueueMapPools
                .Include(x => x.Relationships.MapPool)
                .Where(x => x.Relationships.MatchmakerQueue.Data.Id == selectedMatchmakerQueueId.Value)
                .FetchDataAsync()
                .ContinueWith(x => x.IsFaulted ? null : x.Result.Data
                .Select(x => new MatchmakerQueueMapPool()
                {
                    Id = x.Id,
                    Attributes = x.Attributes,
                    Relationships = x.Relationships
                })
                .ToArray(), TaskScheduler.Default);
        }
        private async Task LoadSelectedMatchmakerQueueMapPoolAsync()
        {
            if (SelectedMatchmakerQueueMapPool == null) return;
            var mapPoolId = SelectedMatchmakerQueueMapPool.Relationships.MapPool.Data.Id;
            MapPoolAssignments = await _fafApi.MapPoolAssignments
                .Include(x => x.Relationships.MapVersion)
                .Where(x => x.Relationships.MapPool.Data.Id == mapPoolId)
                .FetchDataAsync()
                .ContinueWith(x => x.IsFaulted ? null : x.Result.Data
                    .Select(x => new MapPoolAssignment()
                    {
                        Id = x.Id,
                        Attributes = x.Attributes,
                        Relationships = x.Relationships
                    })
                    .ToArray(), TaskScheduler.Default);

        }
    }
}
