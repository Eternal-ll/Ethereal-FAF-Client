using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models.MapsVault;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class MapsViewModel : Base.ViewModel
    {
        private readonly ILogger<MapsViewModel> Logger;
        private readonly ISnackbarService SnackbarService;

        private readonly IFafApiClient FafApiClient;
        private readonly IFafContentClient FafContentClient;

        public MapsViewModel(ILogger<MapsViewModel> logger, ISnackbarService snackbarService, IFafApiClient fafApiClient, IFafContentClient fafContentClient)
        {
            ChangeSortDirectionCommand = new LambdaCommand(OnChangeSortDirectionCommand, CanChangeSortDirectionCommand);
            DownloadMapCommand = new LambdaCommand(OnDownloadMapCommand);

            Maps = new();
            MapsViewSource = new()
            {
                Source = Maps.AsObservable
            };

            Logger = logger;
            SnackbarService = snackbarService;
            FafApiClient = fafApiClient;
            FafContentClient = fafContentClient;
        }

        private readonly ConcurrentObservableCollection<ApiMapModel> Maps;
        private readonly CollectionViewSource MapsViewSource;
        public ICollectionView MapsView => MapsViewSource.View;

        #region PageSize
        private int _PageSize = 15;
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
        public int Pages { get => _Pages;
            set {
                if (Set(ref _Pages, value))
                {
                    AvailablePages = Enumerable.Range(Math.Max(1, CurrentPage - 50), Math.Min(value, CurrentPage + 50)).ToArray();
                }
            }
        }
        #endregion

        #region SortDescriptionsSource
        private SortDescription[] _SortDescriptionsSource;
        public SortDescription[] SortDescriptionsSource
        {
            get => _SortDescriptionsSource;
            set => Set(ref _SortDescriptionsSource, value);
        } 
        #endregion
        #region SelectedSortDescription
        public SortDescription _SelectedSortDescription = new("None", ListSortDirection.Descending);
        public SortDescription SelectedSortDescription
        {
            get => _SelectedSortDescription;
            set
            {
                if (Set(ref _SelectedSortDescription, value))
                {
                    _SelectedSortDirection = value.Direction;
                    OnPropertyChanged(nameof(SelectedSortDirection));
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

        #region MapName
        private string _MapName;
        public string MapName
        {
            get => _MapName;
            set
            {
                if (Set(ref _MapName, value))
                {

                }
            }
        }
        #endregion

        #region Author
        private string _Author;
        public string Author
        {
            get => _Author;
            set
            {
                if (Set(ref _Author, value))
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
        private bool _OnlyRanked = true;
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
            if (!string.IsNullOrWhiteSpace(Author))
            {
                filter.Append($"author.login=={Author};");
            }
            if (!string.IsNullOrWhiteSpace(MapName))
            {
                filter.Append($"displayName=={MapName};");
            }
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

            if (FafApiClient is null) return;

            Sorting sorting = new()
            {
                SortDirection = SelectedSortDirection,
                Parameter = SelectedSortDescription.PropertyName
            };
            var response = await FafApiClient.GetMapsAsync(
                BuildQuery(),
                sorting,
                new Pagination()
            {
                PageNumber = _CurrentPage,
                PageSize = _PageSize,
            },
            include: new string[] { "latestVersion", "author", "reviewsSummary", "statistics" }, cancellationToken: CancellationTokenSource.Token);
            await response.EnsureSuccessStatusCodeAsync();

            if (response.Error is not null)
            {
                //SnackbarService.Show("Warning", response.Error?.Content, Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Caution);
                return;
            }
            response.Content.ParseIncluded(out var entityData);

            Pages = response.Content.Meta.Page.AvaiablePagesCount;

            if (entityData is not null)
            {
                var keys = new List<string>();
                foreach (var item in entityData)
                {
                    if (item.Key is "versions") continue;
                    if (item.Key is "map")
                    {
                        keys.AddRange(item.Value);
                    }
                    else
                    {
                        keys.AddRange(item.Value.Select(d => $"{item.Key}.{d}"));
                    }
                }
                SetSortDescriptionsSource(keys.ToArray());
            }

            Maps.Clear();
            Maps.AddRange(response.Content.Data);


            CancellationTokenSource = null;
        }
        private void SetSortDescriptionsSource(string[] keys)
        {
            var list = new List<SortDescription>
            {
                new SortDescription("None", ListSortDirection.Descending)
            };
            list.AddRange(keys.Select(d => new SortDescription($"{d}", ListSortDirection.Descending)));
            if (SortDescriptionsSource is not null || SortDescriptionsSource?.Length > list.Count) return;
            SortDescriptionsSource = list.ToArray();
            SelectedSortDescription = SortDescriptionsSource[0];
        }

        #region ChangeSortDirectionCommand
        public ICommand ChangeSortDirectionCommand { get; }
        private bool CanChangeSortDirectionCommand(object arg) => arg is ListSortDirection;
        private void OnChangeSortDirectionCommand(object arg) => SelectedSortDirection = (ListSortDirection)arg;
        #endregion

        #region DownloadMap
        public ICommand DownloadMapCommand { get; }
        //private bool CanDownloadMapCommand(object arg) => arg is ApiMapModel map && map.LatestVersion is not null;
        private async void OnDownloadMapCommand(object arg)
        {
            if (arg is not ApiMapModel map)
            {
                return;
            }
            if (map.LatestVersion is null)
            {
                return;
            }
            //try
            //{
            //    await MapsService.EnsureMapExistAsync(Path.GetFileNameWithoutExtension(map.LatestVersion.Filename));
            //}
            //catch (Exception ex)
            //{
            //    //SnackbarService.Timeout = 10000;
            //    //SnackbarService.Show("Exception", $"Failed to download map with exception:\n{ex}", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Secondary);
            //    return;
            //}
            //SnackbarService.Show("Notification", "Downloaded", Wpf.Ui.Common.SymbolRegular.Check20);
        }
        #endregion
    }
}
