using beta.Models.API.MapsVault;
using Ethereal.FAF.API.Client;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ModsViewModel : Base.ViewModel
    {
        private readonly ILogger<MapsViewModel> Logger;
        private IFafApiClient FafApiClient;

        public ModsViewModel(ILogger<MapsViewModel> logger)
        {
            Maps = new();
            MapsViewSource = new()
            {
                Source = Maps.AsObservable
            };

            Logger = logger;
        }

        public void SetFafApiClient(IFafApiClient fafApiClient)
        {
            FafApiClient = fafApiClient;
        }



        private readonly ConcurrentObservableCollection<ApiMapModel> Maps;
        private readonly CollectionViewSource MapsViewSource;
        public ICollectionView MapsView => MapsViewSource.View;

        #region PageSize
        private int _PageSize = 50;
        public int PageSize
        {
            get => _PageSize;
            set
            {
                if (Set(ref _PageSize, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        #region CurrentPage
        private int _CurrentPage = 1;
        public int CurrentPage
        {
            get => _CurrentPage;
            set
            {
                if (Set(ref _CurrentPage, value))
                {
                    AvailablePages = Enumerable.Range(Math.Min(1, value - 50), Math.Min(Pages, value + 50)).ToArray();
                    RunRequest();
                }
            }
        }
        #endregion

        #region AvailablePages
        private int[] _AvailablePages;
        public int[] AvailablePages { get => _AvailablePages; set => Set(ref _AvailablePages, value); }
        #endregion

        public int[] PageSizeSource { get; set; } = new int[] { 5, 10, 15, 30 };

        #region Pages
        private int _Pages;
        public int Pages
        {
            get => _Pages;
            set
            {
                if (Set(ref _Pages, value))
                {
                    AvailablePages = Enumerable.Range(Math.Min(1, CurrentPage - 50), Math.Min(value, CurrentPage + 50)).ToArray();
                }
            }
        }
        #endregion

        public void RunRequest()
        {
            Task.Run(RequestTask);
        }

        private CancellationTokenSource CancellationTokenSource;
        public async Task RequestTask()
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new();

            if (FafApiClient is null) return;
            var response = await FafApiClient.GetMapsAsync(pagination: new()
            {
                PageNumber = _CurrentPage,
                PageSize = _PageSize,
            },
            include: new string[] { "latestVersion","reviewsSummary", "author" }, cancellationToken: CancellationTokenSource.Token);
            await response.EnsureSuccessStatusCodeAsync();
            if (response.Error is not null)
            {
                return;
            }
            response.Content.ParseIncluded();

            Pages = response.Content.Meta.Page.AvaiablePagesCount;

            Maps.Clear();
            Maps.AddRange(response.Content.Data);


            CancellationTokenSource = null;
        }
    }
}
