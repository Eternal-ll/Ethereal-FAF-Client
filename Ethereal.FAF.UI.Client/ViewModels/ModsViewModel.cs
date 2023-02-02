using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models.MapsVault;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ModsViewModel : Base.ViewModel
    {
        private readonly ILogger<MapsViewModel> Logger;
        private readonly SnackbarService SnackbarService;
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly IFafApiClient FafApiClient;

        public ModsViewModel(ILogger<MapsViewModel> logger, SnackbarService snackbarService, IHttpClientFactory httpClientFactory, IFafApiClient fafApiClient)
        {
            ChangeSortDirectionCommand = new LambdaCommand(OnChangeSortDirectionCommand, CanChangeSortDirectionCommand);
            Mods = new();
            ModsViewSource = new()
            {
                Source = Mods.AsObservable
            };

            Logger = logger;
            SnackbarService = snackbarService;
            HttpClientFactory = httpClientFactory;
            FafApiClient = fafApiClient;
        }
        private readonly ConcurrentObservableCollection<Mod> Mods;
        private readonly CollectionViewSource ModsViewSource;
        public ICollectionView ModsView => ModsViewSource.View;

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
                    AvailablePages = Enumerable.Range(Math.Max(1, value - 50), Math.Min(Pages, value + 50)).ToArray();
                    RunRequest();
                }
            }
        }
        #endregion

        #region AvailablePages
        private int[] _AvailablePages;
        public int[] AvailablePages { get => _AvailablePages; set => Set(ref _AvailablePages, value); }
        #endregion

        public int[] PageSizeSource { get; set; } = new int[] { 5, 10, 15, 30, 50, 75, 100, 150 };

        #region Pages
        private int _Pages;
        public int Pages
        {
            get => _Pages;
            set
            {
                if (Set(ref _Pages, value))
                {
                    AvailablePages = Enumerable.Range(Math.Max(1, CurrentPage - 50), Math.Min(value, CurrentPage + 50)).ToArray();
                }
            }
        }
        #endregion

        public string[] SortDescriptionSource { get; } = new string[]
        {
            "None",
            "author",
            "createTime",
            "displayName",
            "recommended",
            "updateTime",
            "latestVersion.createTime",
            "latestVersion.description",
            "latestVersion.downloadUrl",
            "latestVersion.filename",
            "latestVersion.hidden",
            "latestVersion.ranked",
            "latestVersion.thumbnailUrl",
            "latestVersion.type",
            "latestVersion.uid",
            "latestVersion.updateTime",
            "latestVersion.version",
            "uploader.createTime",
            "uploader.login",
            "uploader.updateTime",
            "uploader.userAgent",
            "reviewsSummary.lowerBound",
            "reviewsSummary.negative",
            "reviewsSummary.positive",
            "reviewsSummary.reviews",
            "reviewsSummary.score",
        };
        #region SelectedSortDescription
        public string _SelectedSortDescription = "None";
        public string SelectedSortDescription
        {
            get => _SelectedSortDescription;
            set
            {
                if (Set(ref _SelectedSortDescription, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion
        #region SelectedSortDirection
        private ListSortDirection _SelectedSortDirection;
        public ListSortDirection SelectedSortDirection
        {
            get => _SelectedSortDirection;
            set
            {
                value = value is ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                if (Set(ref _SelectedSortDirection, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        public ModType[] ModTypeSource { get; } = Enum.GetValues<ModType>();
        #region ModType
        private ModType _SelectedModType;
        public ModType SelectedModType
        {
            get => _SelectedModType;
            set
            {
                if (Set(ref _SelectedModType, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        #region ModName
        private string _ModName;
        public string ModName
        {
            get => _ModName;
            set
            {
                if (Set(ref _ModName, value))
                {

                }
            }
        }
        #endregion

        #region Uploder
        private string _Uploder;
        public string Uploder
        {
            get => _Uploder;
            set
            {
                if (Set(ref _Uploder, value))
                {

                }
            }
        }
        #endregion

        #region IsOnlyVisible
        private bool _IsOnlyVisible = true;
        public bool IsOnlyVisible
        {
            get => _IsOnlyVisible;
            set
            {
                if (Set(ref _IsOnlyVisible, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        #region OnlyRanked
        private bool _OnlyRanked = false;
        public bool OnlyRanked
        {
            get => _OnlyRanked;
            set
            {
                if (Set(ref _OnlyRanked, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        public bool IsEnabled => CancellationTokenSource is null;

        private string BuildQuery()
        {
            StringBuilder filter = new();
            if (IsOnlyVisible)
            {
                filter.Append("latestVersion.hidden==false;");
            }
            if (OnlyRanked)
            {
                filter.Append("latestVersion.ranked==true;");
            }
            if (!string.IsNullOrWhiteSpace(Uploder))
            {
                filter.Append($"uploader.login=={Uploder};");
            }
            if (!string.IsNullOrWhiteSpace(ModName))
            {
                filter.Append($"displayName=={ModName};");
            }
            filter.Append($"latestVersion.type=={SelectedModType};");
            if (filter.Length > 0)
            {
                // Remove last ';'
                filter.Remove(filter.Length - 1, 1);
                return filter.ToString();
            }
            return null;
        }

        public void RunRequest()
        {
            Task.Run(RequestTask)
                .ContinueWith(t =>
                {
                    CancellationTokenSource = null;
                    OnPropertyChanged(nameof(IsEnabled));
                });
        }

        private CancellationTokenSource CancellationTokenSource;
        public async Task RequestTask()
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new();

            Sorting sorting = new()
            {
                SortDirection = SelectedSortDirection,
                Parameter = SelectedSortDescription
            };
            var response = await FafApiClient.GetModsAsync(BuildQuery(), sorting, new Pagination()
            {
                PageNumber = _CurrentPage,
                PageSize = _PageSize,
            },
            include: new string[] { "latestVersion", "uploader", "reviewsSummary" }, cancellationToken: CancellationTokenSource.Token);
            await response.EnsureSuccessStatusCodeAsync();

            if (response.Error is not null)
            {
                CancellationTokenSource = null;
                SnackbarService.Timeout = 10000;
                SnackbarService.Show("Warning", response.Error?.Content, Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Caution);
                return;
            }
            response.Content.ParseIncluded();

            if (response.Content.Data.Length > 0)
            {
                var client = HttpClientFactory.CreateClient();
                foreach (var mod in response.Content.Data)
                {
                    var msg = new HttpRequestMessage(new HttpMethod("HEAD"), mod.LatestVersion.ThumbnailUrl);
                    var rsp = await client.SendAsync(msg, CancellationTokenSource.Token);
                    if (!rsp.IsSuccessStatusCode)
                    {
                        mod.LatestVersion.Attributes["thumbnailUrl"] = "https://via.placeholder.com/60x60.png?text=Not%20Found";
                    }
                }
            }

            Pages = response.Content.Meta.Page.AvaiablePagesCount;

            Mods.Clear();
            Mods.AddRange(response.Content.Data);


            CancellationTokenSource = null;
        }

        #region ChangeSortDirectionCommand
        public ICommand ChangeSortDirectionCommand { get; }
        private bool CanChangeSortDirectionCommand(object arg) => arg is ListSortDirection;
        private void OnChangeSortDirectionCommand(object arg) => SelectedSortDirection = (ListSortDirection)arg;
        #endregion
    }
}
