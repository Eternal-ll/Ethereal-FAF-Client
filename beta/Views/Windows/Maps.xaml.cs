using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models.API;
using beta.Models.API.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace beta.Views.Windows
{
    public enum ApiState
    {
        Idle,
        PendingRequest,
        TimeOut
    }
    /// <summary>
    /// Interaction logic for Maps.xaml
    /// </summary>
    public partial class Maps : Window, INotifyPropertyChanged
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

        private readonly IMapsService MapsService;
        private readonly ICacheService CacheService;
        public Maps()
        {
            InitializeComponent();
            DataContext = this;

            MapsService = App.Services.GetService<IMapsService>();
            CacheService = App.Services.GetService<ICacheService>();
            
            BuildQuery();
            DoRequest();

            WidthsList.SelectionChanged += MapSizesSelectionChanged;
            HeightsList.SelectionChanged += MapSizesSelectionChanged;

            Closed += (s, e) =>
            {
                App.Current.Shutdown();
            };

            ResultListBox.SizeChanged += ResultListBox_SizeChanged;

            _SearchForAuthorCommand = new LambdaCommand(OnSearchForAuthorCommand);

            _ShowBigPreviewCommand = new LambdaCommand(OnShowBigPreviewCommand);
            _CloseBigPreviewCommand = new LambdaCommand(OnCloseBigPreviewCommand);

            Resources.Add("SearchForAuthorCommand", _SearchForAuthorCommand);
            Resources.Add("ShowBigPreviewCommand", _ShowBigPreviewCommand);
        }

        private void ResultListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateGridColumns(e.NewSize.Width);
        }

        private void CalculateGridColumns(double size)
        {
            if (!IsGridEnabled) return;

            if (IsDescriptionEnabled)
            {
                if (size < 960) GridColumns = 1;
                else GridColumns = Convert.ToInt32(size / 580);
            }
            else
            {
                if (size < 400) GridColumns = 1;
                else GridColumns = Convert.ToInt32(size / 267);
            }
        }

        private void MapSizesSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
                        //Dispatcher.Invoke(() => ResponseView.Refref());
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
        private string _MapName = string.Empty;
        public string MapName
        {
            get => _MapName;
            set => SetStandardField(ref _MapName, value);
        }
        #endregion

        #region AuthorName
        private string _AuthorName = string.Empty;
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
        private ApiUniversalData _SelectedMap;
        public ApiUniversalData SelectedMap
        {
            get => _SelectedMap;
            set => Set(ref _SelectedMap, value);
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

        #region Cache

        private readonly Dictionary<int, string> AuthorIdToLogin = new();

        #endregion

        #region UI View

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

                    if (!value)
                    {
                        //ResponseViewSource.LiveSortingProperties.Clear();
                        //ResponseView.SortDescriptions.Clear();
                        BuildQuery();
                        DoRequest();
                    }
                    if (value && ResponseView.SortDescriptions.Count == 0)
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
            new SortDescription(nameof(ApiUniversalData.DisplayedName), ListSortDirection.Ascending),
            //new SortDescription(nameof(ApiUniversalData.AuthorLogin), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.MaxPlayers), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.GamesPlayed), ListSortDirection.Ascending),
            //new SortDescription(nameof(ApiUniversalData.IsRanked), ListSortDirection.Ascending),
            //new SortDescription(nameof(ApiUniversalData.IsRecommended), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.Height), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.Width), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.SummaryLowerBound), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.SummaryScore), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.SummaryReviews), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.UpdateTime), ListSortDirection.Ascending),
            new SortDescription(nameof(ApiUniversalData.CreateTime), ListSortDirection.Ascending)
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
        private readonly CollectionViewSource ResponseViewSource = new();
        public ICollectionView ResponseView => ResponseViewSource.View;
        #endregion

        #region IsExtendedViewEnabled
        private bool _IsExtendedViewEnabled;
        public bool IsExtendedViewEnabled
        {
            get => _IsExtendedViewEnabled;
            set
            {
                if (Set(ref _IsExtendedViewEnabled, value))
                {
                    OnPropertyChanged(nameof(ExtendedPanelVisibility));
                }
            }
        }
        #endregion

        public Visibility ExtendedPanelVisibility => IsExtendedViewEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsDescriptionEnabled
        private bool _IsDescriptionEnabled = true;
        public bool IsDescriptionEnabled
        {
            get => _IsDescriptionEnabled;
            set
            {
                if (Set(ref _IsDescriptionEnabled, value))
                {
                    OnPropertyChanged(nameof(DescriptionVisibility));
                    if (IsGridEnabled)
                    {
                        CalculateGridColumns(ResultListBox.ActualWidth);
                    }   
                }
            }
        }
        #endregion

        public Visibility DescriptionVisibility => IsDescriptionEnabled ? Visibility.Visible : Visibility.Collapsed;

        #region IsGridEnabled
        private bool _IsGridEnabled;
        public bool IsGridEnabled
        {
            get => _IsGridEnabled;
            set
            {
                if (Set(ref _IsGridEnabled, value))
                {
                    CalculateGridColumns(ResultListBox.ActualWidth);
                }
            }
        }
        #endregion

        #region GridColumns
        private int _GridColumns;
        public int GridColumns
        {
            get => _GridColumns;
            set => Set(ref _GridColumns, value);
        }
        #endregion

        #endregion

        private Thread RequestThread;
        private bool PageIndexChanged = false;

        private string GetSortProperty() => SelectedSort.PropertyName switch
        {
            nameof(ApiUniversalData.DisplayedName) => "displayName",
            //nameof(ApiUniversalData.AuthorLogin) => "",
            nameof(ApiUniversalData.MaxPlayers) => "latestVersion.maxPlayers",
            nameof(ApiUniversalData.GamesPlayed) => "gamesPlayed",
            //nameof(ApiUniversalData.IsRanked) => "",
            //nameof(ApiUniversalData.IsRecommended) => "",
            nameof(ApiUniversalData.Height) => "latestVersion.width",
            nameof(ApiUniversalData.Width) => "latestVersion.width",
            nameof(ApiUniversalData.SummaryLowerBound) => "reviewsSummary.lowerBound",
            nameof(ApiUniversalData.SummaryScore) => "reviewsSummary.score",
            nameof(ApiUniversalData.SummaryReviews) => "reviewsSummary.reviews",
            nameof(ApiUniversalData.CreateTime) => "latestVersion.createTime",
            nameof(ApiUniversalData.UpdateTime) => "latestVersion.updateTime",
            _ => null,
        };

        private void BuildQuery()
        {
            StringBuilder query = new();

            StringBuilder filter = new();
            if (IsAdvancedModEnabled)
            {

            }
            else
            {
                if (MapName.Length > 0)
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
                    filter.Remove(filter.Length - 1, 1);
                    filter.Append(");");
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
                    filter.Remove(filter.Length - 1, 1);
                    filter.Append(");");
                }

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
                if (sortProperty is not null )
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
        private void Request()
        {
            var query = CurrentQuery;
            Stopwatch watcher = new();
            watcher.Start();
            WebRequest webRequest = WebRequest.Create("https://api.faforever.com/data/map?" + query + "&page[totals]=None&include=author,latestVersion,reviewsSummary");

            try
            {
                var stream = webRequest.GetResponse().GetResponseStream();
                var json = new StreamReader(stream).ReadToEnd();

                var data = JsonSerializer.Deserialize<ApiUniversalResults>(json);

                for (int i = 0; i < data.Data.Length; i++)
                {
                    var map = data.Data[i];

                    #region Loading data Author / Map / Reviews summaries
                    // it is complicated...
                    if (data.Included is not null)
                    {
                        if (map.Relations.Author?.Data is not null)
                        {
                            var authorId = map.Relations.Author.Data.Id;
                            if (AuthorIdToLogin.TryGetValue(authorId, out var login))
                            {
                                map.AuthorLogin = login;
                            }
                            else
                            {
                                map.AuthorData = data.GetAttributesFromIncluded(ApiDataType.player, map.Relations.Author.Data.Id);

                                if (!AuthorIdToLogin.ContainsKey(authorId))
                                {
                                    AuthorIdToLogin.Add(authorId, map.AuthorLogin);
                                }
                            }
                        }

                        if (map.Relations.LatestVersion?.Data is not null)
                        {
                            map.MapData = data.GetAttributesFromIncluded(ApiDataType.mapVersion, map.Relations.LatestVersion.Data.Id);
                        }

                        if (map.Relations.ReviewsSummary?.Data is not null)
                        {
                            map.ReviewsSummaryData = data.GetAttributesFromIncluded(ApiDataType.mapReviewsSummary, map.Relations.ReviewsSummary.Data.Id);
                            if (map.ReviewsSummaryData is not null)
                            {

                            }
                        }
                    }
                    #endregion

                    // Check if installed
                    map.LocalState = MapsService.CheckLocalMap(map.FolderName);

                    // Check if legacy map
                    map.IsLegacyMap = MapsService.IsLegacyMap(map.FolderName);

                    if (map.ThumbnailUrlSmall is not null)
                    {
                        // small map preview
                        Dispatcher.Invoke(() => map.MapSmallPreview = CacheService.GetImage(new(map.ThumbnailUrlSmall), Models.Folder.MapsSmallPreviews),
                            System.Windows.Threading.DispatcherPriority.Background);
                    }

                    if (map.ThumbnailUrlLarge is not null)
                    {
                        // small big preview
                        Dispatcher.Invoke(() => map.MapLargePreview = CacheService.GetImage(new(map.ThumbnailUrlLarge), Models.Folder.MapsLargePreviews),
                            System.Windows.Threading.DispatcherPriority.Background);
                    }

                    if (map.ThumbnailUrlLarge is null && map.ThumbnailUrlSmall is null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            map.MapSmallPreview = App.Current.Resources["QuestionIcon"] as ImageSource;
                            map.MapLargePreview = App.Current.Resources["QuestionIcon"] as ImageSource;
                        });
                    }
                }

                Dispatcher.Invoke(() => ResponseViewSource.Source = data.Data);
                AvailablePagesCount = data.Meta.Page.AvaiablePagesCount;
                
                _PageNumber = data.Meta.Page.PageNumber;
                OnPropertyChanged(nameof(PageNumber));
                PageIndexChanged = false;
                
                TotalRecords = data.Meta.Page.TotalRecords;

                LastQuery = CurrentQuery;
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
            }
        }

        #region Commands

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
    }
}
