using beta.Infrastructure.Commands;
using beta.Infrastructure.Extensions;
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
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        private bool SetSetting<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (!SettingsService.Set(value, propertyName)) return false;
            OnPropertyChanged(propertyName);
            return true;
        }
        private bool SetSetting<T>(T value, [CallerMemberName] string propertyName = null, params string[] properties)
        {
            if (SettingsService.Set(value, propertyName))
            {
                OnPropertyChanged(propertyName);
                foreach (var property in properties)
                {
                    OnPropertyChanged(property);
                }
                return true;
            }
            return false;
        }
        private T GetSetting<T>([CallerMemberName] string propertyName = null) =>
            SettingsService.Get<T>(propertyName);

        //#region Selectable
        //private bool _Selectable;
        //public bool Selectable
        //{
        //    get => _Selectable;
        //    set => Set(ref _Selectable, value);
        //} 
        //#endregion

        public event EventHandler<ApiMapModel> MapSelected;

        private readonly IMapsService MapsService;
        private readonly ICacheService CacheService;
        private readonly NavigationService NavigationService;
        internal ISettingsService SettingsService { get; private set; }


        public MapsView(NavigationService navigationService) : this() => NavigationService = navigationService;
        public MapsView()
        {
            SettingsService = App.Services.GetService<SettingsService<Properties.MapsVMSettings>>();

            MapsService = App.Services.GetService<IMapsService>();
            CacheService = App.Services.GetService<ICacheService>();

            DataContext = this;

            ResponseViewSource = new();
            LastData = new();

            InitializeComponent();
            SelectedSort = SortDescriptions[5];
            BuildQuery();
            DoRequest();

            WidthsList.SelectionChanged += MapSizesSelectionChanged;
            HeightsList.SelectionChanged += MapSizesSelectionChanged;
        }

        private void MapSizesSelectionChanged(object sender, SelectionChangedEventArgs e) => BuildQuery();

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
                    //if (value == ApiState.Idle)
                    //{
                    //    //Dispatcher.Invoke(() => ResponseView.Refresh());
                    //    OnPropertyChanged(nameof(ResponseView));
                    //}
                }
            }
        }
        #endregion

        public bool IsPendingRequest => _Status == ApiState.PendingRequest;
        public bool IsInputBlocked => !IsPendingRequest;

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
            set => Set(ref _TotalRecords, value);
        }
        #endregion

        #region TotalRecordsOnView
        private int _TotalRecordsOnView;
        public int TotalRecordsOnView
        {
            get => _TotalRecordsOnView;
            set => Set(ref _TotalRecordsOnView, value);
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

        #region Settings

        #region UI

        public Visibility MapDetailVisibility => !IsGridViewEnabled ? Visibility.Visible : Visibility.Collapsed;

        public bool IsGridViewEnabled
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value, properties: nameof(MapDetailVisibility)))
                {
                    if (value)
                    {
                        MapDetailsView = null;
                    }
                    else
                    {
                        if (MapDetailsView is null)
                        MapDetailsView = SelectedMap is null ? null : new MapDetailsView(SelectedMap, null, null);
                    }
                    OnPropertyChanged(nameof(MapDetailsView));
                }
            }
        }

        #endregion

        #region Map card

        public Visibility MapLabelsVisibility => IsMapLabelsEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsMapLabelsEnabled
        public bool IsMapLabelsEnabled
        {
            get => GetSetting<bool>();
            set => SetSetting(value, properties: nameof(MapLabelsVisibility));
        }
        #endregion

        public Visibility MapDataVisibility => IsMapDataEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsMapDataEnabled
        public bool IsMapDataEnabled
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value, properties: nameof(MapDataVisibility)))
                {
                    IsMapSummaryEnabled = !value;
                }
            }
        }
        #endregion

        public Visibility MapSummaryVisibility => IsMapSummaryEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsMapSummaryEnabled
        public bool IsMapSummaryEnabled
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value, properties: nameof(MapSummaryVisibility)))
                {
                    IsMapDataEnabled = !value;
                }
            }
        }
        #endregion

        public Visibility MapTitleVisibility => IsMapTitleEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsMapTitleEnabled
        public bool IsMapTitleEnabled
        {
            get => GetSetting<bool>();
            set => SetSetting(value, properties: nameof(MapTitleVisibility));
        }
        #endregion

        public Visibility MapInteractiveButtonVisibility => IsMapInteractiveButtonsEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsMapInteractiveButtonsEnabled
        public bool IsMapInteractiveButtonsEnabled
        {
            get => GetSetting<bool>();
            set => SetSetting(value, properties: nameof(MapInteractiveButtonVisibility));
        }
        #endregion

        public Visibility MapDescriptionVisibility => IsDescriptionEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsDescriptionEnabled
        public bool IsDescriptionEnabled
        {
            get => GetSetting<bool>();
            set => SetSetting(value, properties: nameof(MapDescriptionVisibility));
        }
        #endregion

        #region MapDescriptionOpacity
        public double MapDescriptionOpacity
        {
            get => GetSetting<double>();
            set => SetSetting(value);
        }
        #endregion

        #endregion

        #region Main

        public Visibility QueryVisibility => IsQueryEnabled ? Visibility.Visible : Visibility.Collapsed;
        #region IsQueryEnabled
        public bool IsQueryEnabled
        {
            get => SettingsService.Get<bool>();
            set => SetSetting(value, properties: nameof(QueryVisibility));
        }
        #endregion



        #region IsSortEnabled
        public bool IsSortEnabled
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value, properties: nameof(SortPanelVisibility)))
                {
                    BuildQuery();
                    DoRequest();
                    if (value && ResponseView?.SortDescriptions.Count == 0)
                    {
                        SelectedSort = SortDescriptions[5];
                    }
                }
            }
        }
        #endregion

        public bool IsHiddenMapsDisabled
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value))
                {
                    BuildQuery();
                    DoRequest();
                }
            }
        }

        #region IsOnlyRanked
        public bool IsOnlyRanked
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value))
                {
                    BuildQuery();
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsOnlyLocalMaps
        public bool IsOnlyLocalMaps
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value))
                {
                    BuildQuery();
                    DoRequest();
                }
            }
        }
        #endregion

        #region IsOnlyRecommended
        public bool IsOnlyRecommended
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value))
                {
                    BuildQuery();
                    DoRequest();
                }
            }
        }
        #endregion

        #endregion

        #region Pagination

        #region IsInfinityScrollEnabled
        public bool IsInfinityScrollEnabled
        {
            get => GetSetting<bool>();
            set
            {
                if (SetSetting(value) && value) CheckMaps();
            }
        }
        #endregion

        public Visibility PaginationVisbility => IsPaginationEnabled ? Visibility.Visible : Visibility.Collapsed;
        public bool IsPaginationEnabled
        {
            get => GetSetting<bool>();
            set => SetSetting(value, properties: nameof(PaginationVisbility));
        }

        #endregion

        #endregion

        private void CheckMaps()
        {
            var scroll = MapsScrollViewer;
            var extent = scroll.ExtentHeight;
            var original = scroll.ViewportHeight;
            var offset = scroll.VerticalOffset;

            var difference = extent - original;
            if (difference == 0 || OffsetBottom < 200)
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
        public int[] SelectedMapWidths => WidthsList.SelectedItems.Cast<int>().ToArray();
        public int[] SelectedMapHeights => HeightsList.SelectedItems.Cast<int>().ToArray();
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

        public MapDetailsView MapDetailsView { get; set; }

        #region SelectedMap
        private ApiMapModel _SelectedMap;
        public ApiMapModel SelectedMap
        {
            get => _SelectedMap;
            set
            {
                if (Set(ref _SelectedMap, value))
                {
                    MapSelected?.Invoke(this, value);

                    if (IsGridViewEnabled) return;

                    if (value is null)
                    {
                        MapDetailsView = null;
                    }
                    else
                    {
                        MapDetailsView = new MapDetailsView(value, null, null);
                    }
                    OnPropertyChanged(nameof(MapDetailsView));
                }
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

        #region Sort properties
        public Visibility SortPanelVisibility => IsSortEnabled ? Visibility.Visible : Visibility.Collapsed;

        public static SortDescription[] SortDescriptions => new SortDescription[]
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


        private string _TextDirection;
        public string TextDirection
        {
            get => _TextDirection;
            set => Set(ref _TextDirection, value);
        }

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

                    switch (value.PropertyName)
                    {
                        case nameof(ApiMapData.DisplayedName):
                            TextDirection = value.Direction is ListSortDirection.Descending ? "\uf15d" : "\uf15e";
                        break;
                        case nameof(ApiMapData.MaxPlayers):
                        case nameof(ApiMapData.GamesPlayed):
                        case nameof(ApiMapData.Height):
                        case nameof(ApiMapData.Width):
                        case nameof(ApiMapData.SummaryLowerBound):
                        case nameof(ApiMapData.SummaryScore):
                        case nameof(ApiMapData.SummaryReviews):
                            TextDirection = value.Direction is ListSortDirection.Descending ? "\uf162" : "\uf163";
                            break;
                        case nameof(ApiMapData.UpdateTime):
                        case nameof(ApiMapData.CreateTime):
                            TextDirection = value.Direction is ListSortDirection.Descending ? "\uf884" : "\uf885";
                            break;
                    }
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
            if (SelectedMapHeights.Length > 0)
            {
                filter.Append($"latestVersion.height=in=(");
                for (int i = 0; i < SelectedMapHeights.Length; i++)
                {
                    var num = int.Parse(SelectedMapHeights[i].ToString());
                    filter.Append($"'{Tools.CalculateMapSizeToPixels(num)}',");
                }
                // Remove last ','
                filter[^1] = ')';
                filter.Append(';');
            }
            if (SelectedMapWidths.Length > 0)
            {
                filter.Append($"latestVersion.width=in=(");
                for (int i = 0; i < SelectedMapWidths.Length; i++)
                {
                    var num = int.Parse(SelectedMapWidths[i].ToString());
                    filter.Append($"'{Tools.CalculateMapSizeToPixels(num)}',");
                }
                // Remove last ','
                filter[^1] = ')';
                filter.Append(';');
            }
            IsAnyFilter = filter.Length > 0;
            if (IsHiddenMapsDisabled)
            {
                filter.Append("latestVersion.hidden=='false';");
            }

            if (filter.Length > 0)
            {
                // Remove last ';'
                filter.Remove(filter.Length - 1, 1);

                filter.Insert(0, "filter=(");

                filter.Append(")&");

                var filterQuery = filter.ToString();
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

            RequestThread = new(Request);
            Status = ApiState.PendingRequest;
            RequestThread.Start();

        }
        public ObservableCollection<ApiMapModel> LastData { get; set; }
        private void Request()
        {
            var maps = LastData;
            var query = CurrentQuery;
            Stopwatch watcher = new();
            watcher.Start();
            WebRequest webRequest = WebRequest.Create("https://api.faforever.com/data/map?" + query + "&page[totals]=None&include=author,latestVersion,reviewsSummary");
            if (!(IsInfinityScrollEnabled && PageIndexChanged))
            {
                DispatcherHelper.RunOnMainThread(() => maps.Clear());
            }
            try
            {
                var stream = webRequest.GetResponse().GetResponseStream();
                var json = new StreamReader(stream).ReadToEnd();

                var data = JsonSerializer.Deserialize<ApiMapsResult>(json);
                data.ParseIncluded();

                if (PageSize > 30)
                {
                    var splitted = data.Data.Split(15);

                    foreach (var array in splitted)
                    {
                        var mapsArray = array.ToArray();
                        Dispatcher.Invoke(() =>
                        {
                            foreach (var map in mapsArray)
                            {
                                if (map.LatestVersion is not null)
                                    map.LatestVersion.LocalState = MapsService.CheckLocalMap(map.LatestVersion.FolderName);
                                maps.Add(map);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Background);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var map in data.Data)
                        {
                            if (map.LatestVersion is not null)
                                map.LatestVersion.LocalState = MapsService.CheckLocalMap(map.LatestVersion.FolderName);
                            maps?.Add(map);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }

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

                if (IsInfinityScrollEnabled)
                {
                    //CheckMaps();
                }
            }
        }

        #region Commands

        #region CleanDataCommand
        private ICommand _CleanDataCommand;
        public ICommand CleanDataCommand => _CleanDataCommand ??= new LambdaCommand(OnCleanDataCommand, CanCleanDataCommand);
        private bool CanCleanDataCommand(object parameter) => LastData is not null && LastData.Any();
        private void OnCleanDataCommand(object parameter)
        {
            LastData.Clear();
            //LastData = null;
            UpdateLayout();
            GC.Collect();
            //LastData = new();
            OnPropertyChanged(nameof(LastData));
        }
        #endregion

        #region OpenDetailsCommand
        private ICommand _OpenDetailsCommand;
        public ICommand OpenDetailsCommand => _OpenDetailsCommand ??= new LambdaCommand(OnOpenDetailsCommand);
        private void OnOpenDetailsCommand(object parameter)
        {
            if (parameter is null) return;
            if (parameter is ApiMapModel map)
            {
                if (map.LatestVersion is null) return;

                if (!IsGridViewEnabled)
                {
                    SelectedMap = map;
                    return;
                }

                MapSelected?.Invoke(this, map);
                if (IsAnyFilter)
                {
                    var maps = new List<ApiMapModel>(LastData);
                    maps.Remove(map);
                    NavigationService?.Navigate(new MapDetailsView(map, maps.ToArray(), NavigationService));
                }
                else
                {
                    NavigationService?.Navigate(new MapDetailsView(map, null, NavigationService));
                }
            }
        }
        #endregion

        #region SearchForAuthorCommand
        private ICommand _SearchForAuthorCommand;
        public ICommand SearchForAuthorCommand => _SearchForAuthorCommand ??= new LambdaCommand(OnSearchForAuthorCommand);

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

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e) => DoRequest();
        private void RemoveMinClick(object sender, RoutedEventArgs e) => MinimumSlots = null;
        private void RemoveMaxClick(object sender, RoutedEventArgs e) => MaximumSlots = null;

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

                Dispatcher.Invoke(() =>
                {
                    sender.IsSuggestionListOpen = res.Length > 0;
                    sender.ItemsSource = res.OrderBy(x => x.StartsWith(query, StringComparison.OrdinalIgnoreCase));
                    if (!SuggestionCache.ContainsKey(query))
                        SuggestionCache.Add(query, res);
                });
                PendingQuery = false;
            });
        }

        private ScrollViewer MapsScrollViewer { get; set; }
        private double OffsetBottom = 0;
        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange < 0) return;
            var extent = e.ExtentHeight;
            var original = e.ViewportHeight;
            var offset = e.VerticalOffset;

            var originalNew = original + offset;
            var difference = extent - originalNew;
            OffsetBottom = difference;

            var min = 200;
            if (!IsGridViewEnabled)
            {
                min = 10;
            }

            if (difference < min)
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

        private void ResponseListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var scroll = Tools.FindChild<ScrollViewer>(listBox);
            scroll.ScrollChanged += Scroll_ScrollChanged;
            MapsScrollViewer = scroll;
        }
    }
}
