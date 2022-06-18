using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models.API;
using beta.Models.API.Base;
using beta.Models.API.MapsVault;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;

namespace beta.Views
{
    public enum ApiState
    {
        Idle,
        PendingRequest,
        TimeOut
    }
    /// <summary>
    /// Interaction logic for MapsView.xaml
    /// </summary>
    public partial class MapsView : UserControl, INotifyPropertyChanged
    {
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
        #endregion

        public bool Selectable { get; set; }

        public event EventHandler<ApiMapModel> MapSelected;

        private readonly IMapsService MapsService;
        private readonly ICacheService CacheService;
        private readonly NavigationService NavigationService;
        public MapsView(NavigationService navigationService) : this() => NavigationService = navigationService;
        public MapsView()
        {
            InitializeComponent();
            DataContext = this;

            MapsService = App.Services.GetService<IMapsService>();
            CacheService = App.Services.GetService<ICacheService>();

            ResponseViewSource = new();
            LastData = new();
            //ResponseViewSource.Source = last

            BuildQuery();
            DoRequest();

            WidthsList.SelectionChanged += MapSizesSelectionChanged;
            HeightsList.SelectionChanged += MapSizesSelectionChanged;

            _SearchForAuthorCommand = new LambdaCommand(OnSearchForAuthorCommand);

            _ShowBigPreviewCommand = new LambdaCommand(OnShowBigPreviewCommand);
            _CloseBigPreviewCommand = new LambdaCommand(OnCloseBigPreviewCommand);

            Resources.Add("SearchForAuthorCommand", _SearchForAuthorCommand);
            Resources.Add("ShowBigPreviewCommand", _ShowBigPreviewCommand);
            Resources.Add(nameof(OpenDetailsCommand), OpenDetailsCommand);
        }

        public MapsView(bool selectable): base()
        {
            Selectable = selectable;
        }
        private void MapSizesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuildQuery();
        }

        #region Status
        private ApiState _Status;
        public ApiState Status
        {
            get => _Status;
            set
            {
                if (Set(ref _Status, value))
                {
                    OnPropertyChanged(nameof(IsPendingRequest));
                    OnPropertyChanged(nameof(IsInputBlocked));
                    OnPropertyChanged(nameof(IsResultListVisible));
                    if (value == ApiState.Idle)
                    {
                        //Dispatcher.Invoke(() => ResponseView.Refresh());
                        OnPropertyChanged(nameof(ResponseView));
                    }
                }
            }
        }
        #endregion

        public bool IsPendingRequest => _Status == ApiState.PendingRequest;
        public bool IsInputBlocked => !IsPendingRequest;
        public Visibility IsResultListVisible => _Status == ApiState.Idle ? Visibility.Visible : Visibility.Collapsed;

        #region BigPreviewVisibility
        private Visibility _BigPreviewVisibility = Visibility.Collapsed;
        public Visibility BigPreviewVisibility
        {
            get => _BigPreviewVisibility;
            set => Set(ref _BigPreviewVisibility, value);
        }
        #endregion

        #region Result page properties
        #region PageNumber
        private int _PageNumber = 1;
        public int PageNumber
        {
            get => _PageNumber;
            set
            {
                if (value > AvailablePagesCount) value = 1;
                else if (value == 0) value = AvailablePagesCount;

                if (SetStandardField(ref _PageNumber, value))
                {
                    PageIndexChanged = true;
                    if (AvailablePagesCount != 0) DoRequest();
                }
            }
        }
        #endregion

        #region AvailablePages
        public int[] AvailablePages => Enumerable.Range(1, AvailablePagesCount).ToArray();

        #region AvailablePagesCount
        private int _AvailablePagesCount;
        public int AvailablePagesCount
        {
            get => _AvailablePagesCount;
            set
            {
                if (Set(ref _AvailablePagesCount, value))
                {
                    OnPropertyChanged(nameof(AvailablePages));
                }
            }
        }
        #endregion
        #endregion

        #region AvaiablePageSizes
        private readonly int[] _AvaiablePageSizes = new int[]
        {
            5, 10, 15, 25, 50, 75, 100, 200
        };
        public int[] AvaiablePageSizes => _AvaiablePageSizes;

        #endregion

        #region PageSize
        private int _PageSize = 15;
        public int PageSize
        {
            get => _PageSize;
            set
            {
                if (SetStandardField(ref _PageSize, value))
                {
                    //PageNumber = 1;
                    DoRequest();
                }
            }
        }
        #endregion

        #region TotalRecords
        private int _TotalRecords;
        public int TotalRecords
        {
            get => _TotalRecords;
            set
            {
                if (Set(ref _TotalRecords, value))
                {
                    //PageNumber = 1;
                }
            }
        }
        #endregion
        #endregion

        #region Elapsed
        private TimeSpan _Elapsed;
        public TimeSpan Elapsed
        {
            get => _Elapsed;
            set => Set(ref _Elapsed, value);
        }
        #endregion

        #region IsUIMod
        private bool _IsUIMod;
        public bool IsUIMod
        {
            get => _IsUIMod;
            set
            {
                if (Set(ref _IsUIMod, value))
                {
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsSimMod
        private bool _IsSimMod;
        public bool IsSimMod
        {
            get => _IsSimMod;
            set
            {
                if (Set(ref _IsSimMod, value))
                {
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsOnlyRanked
        private bool _IsOnlyRanked;
        public bool IsOnlyRanked
        {
            get => _IsOnlyRanked;
            set
            {
                if (SetStandardField(ref _IsOnlyRanked, value))
                {
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsOnlyRecommended
        private bool _IsOnlyRecommended;
        public bool IsOnlyRecommended
        {
            get => _IsOnlyRecommended;
            set
            {
                if (SetStandardField(ref _IsOnlyRecommended, value))
                {
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsOnlyLocalMaps
        private bool _IsOnlyLocalMaps;
        public bool IsOnlyLocalMaps
        {
            get => _IsOnlyLocalMaps;
            set
            {
                if (SetStandardField(ref _IsOnlyLocalMaps, value))
                {
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsAdvancedModEnabled
        private bool _IsAdvancedModEnabled;
        public bool IsAdvancedModEnabled
        {
            get => _IsAdvancedModEnabled;
            set
            {
                if (Set(ref _IsAdvancedModEnabled, value))
                {
                    OnPropertyChanged(nameof(StandardFormVisibility));
                }
            }
        }
        #endregion

        #region IsInfinityScrollEnabled
        private bool _IsInfinityScrollEnabled;
        public bool IsInfinityScrollEnabled
        {
            get => _IsInfinityScrollEnabled;
            set
            {
                if (Set(ref _IsInfinityScrollEnabled, value) && value)
                {
                    CheckMaps();
                }
            }
        }
        #endregion

        #region UI

        #region Map card settings

        public Visibility MapLabelsVisibility => IsMapLabelsEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsMapLabelsEnabled
        private bool _IsMapLabelsEnabled;
        public bool IsMapLabelsEnabled
        {
            get => _IsMapLabelsEnabled;
            set
            {
                if (Set(ref _IsMapLabelsEnabled, value))
                {
                    OnPropertyChanged(nameof(MapLabelsVisibility));
                }
            }
        }
        #endregion

        public Visibility MapDataVisibility => IsMapDataEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsMapDataEnabled
        private bool _IsMapDataEnabled;
        public bool IsMapDataEnabled
        {
            get => _IsMapDataEnabled;
            set
            {
                if (Set(ref _IsMapDataEnabled, value))
                {
                    OnPropertyChanged(nameof(MapDataVisibility));
                    IsMapSummaryEnabled = !value;
                }
            }
        }
        #endregion

        public Visibility MapSummaryVisibility => IsMapSummaryEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsMapSummaryEnabled
        private bool _IsMapSummaryEnabled = true;
        public bool IsMapSummaryEnabled
        {
            get => _IsMapSummaryEnabled;
            set
            {
                if (Set(ref _IsMapSummaryEnabled, value))
                {
                    OnPropertyChanged(nameof(MapSummaryVisibility));
                    IsMapDataEnabled = !value;
                }
            }
        }
        #endregion

        public Visibility MapTitleVisibility => IsMapTitleEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsMapTitleEnabled
        private bool _IsMapTitleEnabled;
        public bool IsMapTitleEnabled
        {
            get => _IsMapTitleEnabled;
            set
            {
                if (Set(ref _IsMapTitleEnabled, value))
                {
                    OnPropertyChanged(nameof(MapTitleVisibility));
                }
            }
        }
        #endregion

        public Visibility MapInteractiveButtonVisibility => IsMapInteractiveButtonsEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsMapInteractiveButtonsEnabled
        private bool _IsMapInteractiveButtonsEnabled;
        public bool IsMapInteractiveButtonsEnabled
        {
            get => _IsMapInteractiveButtonsEnabled;
            set
            {
                if (Set(ref _IsMapInteractiveButtonsEnabled, value))
                {
                    OnPropertyChanged(nameof(MapInteractiveButtonVisibility));
                }
            }
        }
        #endregion

        public Visibility MapDescriptionVisibility => IsDescriptionEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsDescriptionEnabled
        private bool _IsDescriptionEnabled;
        public bool IsDescriptionEnabled
        {
            get => _IsDescriptionEnabled;
            set
            {
                if (Set(ref _IsDescriptionEnabled, value))
                {
                    OnPropertyChanged(nameof(MapDescriptionVisibility));
                }
            }
        }
        #endregion

        #region MapDescriptionOpacity
        private double _MapDescriptionOpacity = .6;
        public double MapDescriptionOpacity
        {
            get => _MapDescriptionOpacity;
            set => Set(ref _MapDescriptionOpacity, value);
        }
        #endregion

        #endregion

        #endregion

        private void CheckMaps()
        {
            var scroll = MapsScrollViewer;
            var extent = scroll.ExtentHeight;
            var original = scroll.ViewportHeight;

            var difference = extent - original;
            if (difference == 0)
            {
                if (IsPendingRequest) return;
                AppendNewData = true;
                if (AvailablePagesCount > PageNumber)
                {
                    PageNumber++;
                    DoRequest();
                }
            }
        }

        private bool AppendNewData;

        #region MapSizes
        private readonly int[] _MapSizes = new int[]
        {
            1, 2, 5, 10, 20, 40, 80
        };
        public int[] MapSizes => _MapSizes;
        #endregion

        #region MapSlots
        private readonly int?[] _MapSlots = new int?[]
        {
            1 , 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16
        };
        public int?[] MapSlots => _MapSlots;

        #endregion

        #region Standard mode fields

        public Visibility StandardFormVisibility => !IsAdvancedModEnabled ? Visibility.Visible : Visibility.Collapsed;

        private bool SetStandardField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Set(ref field, value, propertyName))
            {
                BuildQuery();
                return true;
            }
            return false;
        }

        #region MapName
        private string _MapName;
        public string MapName
        {
            get => _MapName;
            set => SetStandardField(ref _MapName, value);
        }
        #endregion

        #region AuthorName
        private string _AuthorName;
        public string AuthorName
        {
            get => _AuthorName;
            set => SetStandardField(ref _AuthorName, value);
        }
        #endregion

        #region Selected map sizes
        public IList SelectedMapWidths
        {
            get
            {
                IList t = new List<int>();
                Dispatcher.Invoke(() => t = WidthsList.SelectedItems);
                return t;
            }
        }
        public IList SelectedMapHeights
        {
            get
            {
                IList t = new List<int>();
                Dispatcher.Invoke(() => t = HeightsList.SelectedItems);
                return t;
            }
        }
        #endregion

        #region MinimumSlots
        private int? _MinimumSlots;
        public int? MinimumSlots
        {
            get => _MinimumSlots;
            set => SetStandardField(ref _MinimumSlots, value);
        }
        #endregion

        #region MaximumSlots
        private int? _MaximumSlots;
        public int? MaximumSlots
        {
            get => _MaximumSlots;
            set => SetStandardField(ref _MaximumSlots, value);
        }
        #endregion

        #endregion

        #region SelectedMap
        private ApiMapModel _SelectedMap;
        public ApiMapModel SelectedMap
        {
            get => _SelectedMap;
            set
            {
                //if (Set(ref _SelectedMap, value))
                //{
                //    MapSelected?.Invoke(this, value);
                //}
            }
        }
        #endregion

        #region LastQuery
        private string _LastQuery = String.Empty;
        public string LastQuery
        {
            get => _LastQuery;
            set => Set(ref _LastQuery, value);
        }
        #endregion

        #region CurrentQuery
        private string _CurrentQuery;
        public string CurrentQuery
        {
            get => _CurrentQuery;
            set => Set(ref _CurrentQuery, value);
        }
        #endregion

        #region IsSortEnabled
        private bool _IsSortEnabled;
        public bool IsSortEnabled
        {
            get => _IsSortEnabled;
            set
            {
                if (Set(ref _IsSortEnabled, value))
                {
                    OnPropertyChanged(nameof(SortPanelVisibility));

                    BuildQuery();
                    DoRequest();
                    if (value && ResponseView?.SortDescriptions.Count == 0)
                    {
                        SelectedSort = SortDescriptions[0];
                    }
                }
            }
        }
        #endregion

        #region Sort properties

        public Visibility SortPanelVisibility => IsSortEnabled ? Visibility.Visible : Visibility.Collapsed;

        public SortDescription[] SortDescriptions => new SortDescription[]
        {
            new SortDescription(nameof(ApiMapData.DisplayedName), ListSortDirection.Ascending),
            //new SortDescription(nameof(ApiUniversalData.AuthorLogin), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.MaxPlayers), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.GamesPlayed), ListSortDirection.Ascending),
            //new SortDescription(nameof(ApiUniversalData.IsRanked), ListSortDirection.Ascending),
            //new SortDescription(nameof(ApiUniversalData.IsRecommended), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.Height), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.Width), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.SummaryLowerBound), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.SummaryScore), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.SummaryReviews), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.UpdateTime), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiMapData.CreateTime), ListSortDirection.Ascending)
        };

        #region SelectedSort
        private SortDescription _SelectedSort;
        public SortDescription SelectedSort
        {
            get => _SelectedSort;
            set
            {
                if (SetStandardField(ref _SelectedSort, value))
                {
                    OnPropertyChanged(nameof(SortDirection));

                    if (IsSortEnabled)
                    {
                        DoRequest();
                    }

                    //CollectionViewSource.LiveSortingProperties.Clear();
                    //CollectionViewSource.LiveSortingProperties.Add(value.PropertyName);
                    //SortDirection = ListSortDirection.Ascending;
                    //ResponseView.SortDescriptions.Clear();
                    //ResponseView.SortDescriptions.Add(value);
                }
            }
        }
        #endregion

        #region SortDirection
        public ListSortDirection SortDirection
        {
            get => SelectedSort.Direction;
            set
            {
                if (SelectedSort.Direction == ListSortDirection.Ascending)
                {
                    SelectedSort = new(SelectedSort.PropertyName, ListSortDirection.Descending);
                }
                else SelectedSort = new(SelectedSort.PropertyName, ListSortDirection.Ascending);
            }
        }
        #endregion

        #region ChangeSortDirectionCommmand
        private ICommand _ChangeSortDirectionCommmand;
        public ICommand ChangeSortDirectionCommmand => _ChangeSortDirectionCommmand ??= new LambdaCommand(OnChangeSortDirectionCommmand);
        public void OnChangeSortDirectionCommmand(object parameter) => SortDirection = ListSortDirection.Ascending;
        #endregion

        #endregion

        #region ResponseView
        private readonly CollectionViewSource ResponseViewSource;
        public ICollectionView ResponseView => ResponseViewSource.View;
        #endregion

        private Thread RequestThread;
        private bool PageIndexChanged = false;

        private string GetSortProperty() => SelectedSort.PropertyName switch
        {
            nameof(ApiMapData.DisplayedName) => "displayName",
            //nameof(ApiUniversalData.AuthorLogin) => "",
            nameof(ApiMapData.MaxPlayers) => "latestVersion.maxPlayers",
            nameof(ApiMapData.GamesPlayed) => "gamesPlayed",
            //nameof(ApiUniversalData.IsRanked) => "",
            //nameof(ApiUniversalData.IsRecommended) => "",
            nameof(ApiMapData.Height) => "latestVersion.width",
            nameof(ApiMapData.Width) => "latestVersion.width",
            nameof(ApiMapData.SummaryLowerBound) => "reviewsSummary.lowerBound",
            nameof(ApiMapData.SummaryScore) => "reviewsSummary.score",
            nameof(ApiMapData.SummaryReviews) => "reviewsSummary.reviews",
            nameof(ApiMapData.CreateTime) => "createTime",
            nameof(ApiMapData.UpdateTime) => "updateTime",
            _ => null,
        };

        private bool IsAnyFilter = false;

        private void BuildQuery()
        {
            StringBuilder query = new();

            StringBuilder filter = new();
            if (IsAdvancedModEnabled)
            {

            }
            else
            {
                if (IsOnlyLocalMaps)
                {
                    var maps = MapsService.GetLocalMaps();
                    filter.Append($"latestVersion.folderName=in=(");
                    
                    foreach (var map in maps)
                    {
                        filter.Append($"'{map}',");
                    }
                    filter[^1] = ')';
                    filter.Append(';');
                }
                if (MapName?.Length > 0)
                {
                    filter.Append($"displayName=='{MapName}';");
                }

                if (AuthorName?.Length > 0)
                {
                    filter.Append($"author.login=='{AuthorName}';");
                }

                if (IsOnlyRanked)
                {
                    filter.Append("latestVersion.ranked=='true';");
                }

                if (IsOnlyRecommended)
                {
                    filter.Append("recommended=='true';");
                }

                if (MinimumSlots is not null)
                {
                    filter.Append($"latestVersion.maxPlayers=ge='{MinimumSlots}';");
                }

                if (MaximumSlots is not null)
                {
                    filter.Append($"latestVersion.maxPlayers=le='{MaximumSlots}';");
                }

                if (SelectedMapHeights.Count > 0)
                {
                    filter.Append($"latestVersion.height=in=(");
                    for (int i = 0; i < SelectedMapHeights.Count; i++)
                    {
                        var num = int.Parse(SelectedMapHeights[i].ToString());
                        filter.Append($"'{Tools.CalculateMapSizeToPixels(num)}',");
                    }
                    // Remove last ','
                    filter[^1] = ')';
                    filter.Append(';');
                }

                if (SelectedMapWidths.Count > 0)
                {
                    filter.Append($"latestVersion.width=in=(");
                    for (int i = 0; i < SelectedMapWidths.Count; i++)
                    {
                        var num = int.Parse(SelectedMapWidths[i].ToString());
                        filter.Append($"'{Tools.CalculateMapSizeToPixels(num)}',");
                    }
                    // Remove last ','
                    filter[^1] = ')';
                    filter.Append(';');
                }
                IsAnyFilter = filter.Length > 0;
                // 
                filter.Append("latestVersion.hidden=='false';");

                if (filter.Length > 0)
                {
                    // Remove last ';'
                    filter.Remove(filter.Length - 1, 1);

                    filter.Insert(0, "filter=(");

                    filter.Append(")&");

                    var filterQuery = filter.ToString();
                }
            }

            if (filter.Length > 0)
            {
                query.Append(filter);
            }

            StringBuilder sort = new();
            if (IsSortEnabled)
            {
                var sortProperty = GetSortProperty();
                if (sortProperty is not null)
                {
                    query.Append("sort=");
                    if (SelectedSort.Direction == ListSortDirection.Descending)
                    {
                        query.Append('-');
                    }
                    query.Append(sortProperty);
                    query.Append('&');
                }
            }

            query.Append($"page[size]={PageSize}");

            string fullQuery = query.ToString();

            CurrentQuery = query.ToString();
        }
        private void DoRequest()
        {
            if (RequestThread is not null)
            {
                return;
            }

            if (LastQuery == CurrentQuery)  
            {
                if (!PageIndexChanged) return;
            }
            else
            {
                if (!PageIndexChanged)
                {
                    _PageNumber = 1;
                }
            }

            CurrentQuery += $"&page[number]={PageNumber}";

            //SelectedMap = null;

            RequestThread = new(Request);
            Status = ApiState.PendingRequest;
            RequestThread.Start();

        }
        private readonly ObservableCollection<ApiMapData> LocalMaps = new();
        public ObservableCollection<ApiMapModel> LastData { get; set; }
        private void Request()
        {
            var maps = LastData;
            var query = CurrentQuery;
            Stopwatch watcher = new();
            watcher.Start();
            WebRequest webRequest = WebRequest.Create("https://api.faforever.com/data/map?" + query + "&page[totals]=None&include=author,latestVersion,reviewsSummary");
            if (!IsInfinityScrollEnabled)
            {
                DispatcherHelper.RunOnMainThread(() => maps.Clear());
            }
            try
            {
                var stream = webRequest.GetResponse().GetResponseStream();
                var json = new StreamReader(stream).ReadToEnd();

                var data = JsonSerializer.Deserialize<ApiMapsResult>(json);
                data.ParseIncluded();

                DispatcherHelper.RunOnMainThread(() =>
                {
                    foreach (var map in data.Data)
                    {
                        map.LatestVersion.LocalState = MapsService.CheckLocalMap(map.LatestVersion.FolderName);
                        maps.Add(map);
                    }
                });

                AvailablePagesCount = data.Meta.Page.AvaiablePagesCount;
                _PageNumber = data.Meta.Page.PageNumber;
                OnPropertyChanged(nameof(PageNumber));
                PageIndexChanged = false;
                TotalRecords = data.Meta.Page.TotalRecords;
                LastQuery = query;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                Elapsed = watcher.Elapsed;
                Status = ApiState.Idle;
                RequestThread = null;

                //if (IsInfinityScrollEnabled)
                //{
                //    CheckMaps();
                //}
            }
        }

        #region Commands

        #region OpenDetailsCommand
        private ICommand _OpenDetailsCommand;
        public ICommand OpenDetailsCommand => _OpenDetailsCommand ??= new LambdaCommand(OnOpenDetailsCommand);
        private void OnOpenDetailsCommand(object parameter)
        {
            if (parameter is null) return;
            if (parameter is int id)
            {
                var selected = LastData.First(m => m.Id == id);

                if (Selectable)
                {
                    MapSelected?.Invoke(this, selected);
                    return;
                }
                if (IsAnyFilter)
                {
                    var maps = new List<ApiMapModel>(LastData);
                    maps.Remove(selected);
                    NavigationService?.Navigate(new MapDetailsView(selected, maps.ToArray(), NavigationService));
                }
                else
                {
                    NavigationService?.Navigate(new MapDetailsView(selected, null, NavigationService));
                }
            }
        }
        #endregion

        #region SearchForAuthorCommand
        private ICommand _SearchForAuthorCommand;

        private void OnSearchForAuthorCommand(object parameter)
        {
            if (parameter is null) return;

            if (parameter.ToString().Length == 0) return;
            if (parameter.ToString() == "Unknown") return;

            AuthorName = parameter.ToString();
            BuildQuery();
            DoRequest();
        }
        #endregion

        #region SearchForSameSizeCommand
        private ICommand _SearchForSameSizeCommand;

        private void OnSearchForSameSizeCommand(object parameter)
        {
            WidthsList.SelectedItem = Tools.CalculateMapSizeToPixels(int.Parse(parameter.ToString()));
            HeightsList.SelectedItem = Tools.CalculateMapSizeToPixels(int.Parse(parameter.ToString()));
            BuildQuery();
            DoRequest();
        }
        #endregion

        #region CloseBigPreviewCommand
        private ICommand _CloseBigPreviewCommand;
        public ICommand CloseBigPreviewCommand => _CloseBigPreviewCommand;

        private void OnCloseBigPreviewCommand(object parameter) => BigPreviewVisibility = Visibility.Collapsed;
        #endregion

        #region ShowBigPreviewCommand
        private ICommand _ShowBigPreviewCommand;
        public ICommand ShowBigPreviewCommand => _ShowBigPreviewCommand;

        private void OnShowBigPreviewCommand(object parameter) => BigPreviewVisibility = Visibility.Visible;
        #endregion


        #endregion

        private void Button_Click(object sender, RoutedEventArgs e) => DoRequest();
        private void PreviousPageClick(object sender, RoutedEventArgs e)
        {
            if (1 >= PageNumber)
            {
                PageNumber = AvailablePagesCount;
                return;
            }

            PageNumber--;
        }
        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            if (PageNumber >= AvailablePagesCount)
            {
                PageNumber = 1;
                return;
            }

            PageNumber++;
        }
        private void RemoveMinClick(object sender, RoutedEventArgs e) => MinimumSlots = null;
        private void RemoveMaxClick(object sender, RoutedEventArgs e) => MaximumSlots = null;
        private void OnCurrentPageMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                PageNumber--;
            }
            else
            {
                PageNumber++;
            }
        }

        Dictionary<string, string[]> SuggestionCache = new();
        private bool PendingQuery = false;
        private readonly HttpClient httpClient = new HttpClient();
        private CancellationTokenSource cancel;
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            var query = sender.Text;
            MapName = query;
            if (args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(query))
            {
                sender.IsSuggestionListOpen = false;
                sender.ItemsSource = null;
                PendingQuery = false;
                return;
            }
            else
            {
                if (query[0] != '*') query = '*' + query;
                if (query[^1] != '*') query += '*';
                MapName = query;
                if (SuggestionCache.TryGetValue(query, out var cached))
                {
                    sender.IsSuggestionListOpen = true;
                    sender.ItemsSource = cached;
                    return;
                }
            }
            if (PendingQuery) return;
            PendingQuery = true;
            string queryy = $"https://api.faforever.com/data/map?fields[map]=displayName&filter=(displayName=='{query}';latestVersion.hidden=='false')&page[limit]=10&page[totals]=true";
            if (cancel is not null) cancel.Cancel();
            Task.Run(async () =>
            {
                cancel = new();
                var response = await httpClient.GetAsync(queryy, cancel.Token);
                var data = await JsonSerializer.DeserializeAsync<ApiUniversalResult<ApiUniversalData[]>>(await response.Content.ReadAsStreamAsync(cancel.Token));
                string[] res = new string[data.Data.Length == 0 ? 1 : data.Data.Length];
                res[0] = "No results found";
                for (int i = 0; i < data.Data.Length; i++)
                {
                    var map = data.Data[i];
                    res[i] = map.Attributes["displayName"];
                }
                res = res.OrderBy(x => x.Length).ToArray();
                Dispatcher.Invoke(() =>
                {
                    sender.IsSuggestionListOpen = res.Length > 0;
                    sender.ItemsSource = res;
                    if (!SuggestionCache.ContainsKey(query))
                        SuggestionCache.Add(query, res);
                });
                PendingQuery = false;
            });
        }

        private ScrollViewer MapsScrollViewer { get; set; }
        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listbox = (ListBox)sender;
            var scroll = Tools.FindChild<ScrollViewer>(listbox);
            scroll.ScrollChanged += Scroll_ScrollChanged;
            MapsScrollViewer = scroll;
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange < 0) return;
            var extent = e.ExtentHeight;
            var original = e.ViewportHeight;
            var offset = e.VerticalOffset;

            var originalNew = original + offset;
            var difference = extent - originalNew;
            if (difference < 255)
            {
                if (!IsInfinityScrollEnabled) return; 
                if (IsPendingRequest) return;
                AppendNewData = true;
                if (AvailablePagesCount > PageNumber)
                {
                    PageNumber++;
                    DoRequest();
                }
            }
        }
    }
}
