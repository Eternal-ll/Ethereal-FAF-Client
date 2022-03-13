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
            HeigthsList.SelectionChanged += MapSizesSelectionChanged;
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
                    OnPropertyChanged(nameof(IsResultListVisilbe));
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
        public Visibility IsResultListVisilbe => _Status == ApiState.Idle ? Visibility.Visible : Visibility.Collapsed;

        #region IsAutoRequestEnabled
        private bool _IsAutoRequestEnabled;
        public bool IsAutoRequestEnabled
        {
            get => _IsAutoRequestEnabled;
            set => Set(ref _IsAutoRequestEnabled, value);
        }
        #endregion

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

        public int[] AvailablePages => Enumerable.Range(1, AvailablePagesCount).ToArray();

        #region AvailablePages
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
                Dispatcher.Invoke(() => t = HeigthsList.SelectedItems);
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

        #region Results ICollection view
        private readonly CollectionViewSource ResponseViewSource = new();
        public ICollectionView ResponseView => ResponseViewSource.View; 
        #endregion

        private Thread RequestThread;
        private bool PageIndexChanged = false;

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

                if (AuthorName.Length > 0)
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

                if (MinimumSlots != null)
                {
                    filter.Append($"latestVersion.maxPlayers=ge='{MinimumSlots}';");
                }

                if (MaximumSlots != null)
                {
                    filter.Append($"latestVersion.maxPlayers=le='{MaximumSlots}';");
                }

                if (SelectedMapHeights.Count > 0)
                {
                    filter.Append($"latestVersion.height=in=(");
                    for (int i = 0; i < SelectedMapHeights.Count; i++)
                    {
                        var num = int.Parse(SelectedMapHeights[i].ToString());
                        filter.Append($"'{Tools.CalculateMapSizeInPixels(num)}',");
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
                        filter.Append($"'{Tools.CalculateMapSizeInPixels(num)}',");
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

            query.Append($"page[size]={PageSize}");

            string fullQuery = query.ToString();

            CurrentQuery = query.ToString();
        }
        private void DoRequest()
        {
            if (RequestThread != null)
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
                    if (data.Included != null)
                    {
                        if (map.Relations.Author?.Data != null)
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

                        if (map.Relations.LatestVersion?.Data != null)
                        {
                            map.MapData = data.GetAttributesFromIncluded(ApiDataType.mapVersion, map.Relations.LatestVersion.Data.Id);
                        }

                        if (map.Relations.ReviewsSummary?.Data != null)
                        {
                            map.ReviewsSummaryData = data.GetAttributesFromIncluded(ApiDataType.mapReviewsSummary, map.Relations.ReviewsSummary.Data.Id);
                            if (map.ReviewsSummaryData != null)
                            {

                            }
                        }
                    }
                    #endregion

                    #region Check if installed
                    map.LocalState = MapsService.CheckLocalMap(map.FolderName);
                    #endregion

                    // small map preview
                    Dispatcher.Invoke(() => map.MapSmallPreview = CacheService.GetImage(new(map.ThumbnailUrlSmall), Models.Folder.MapsSmallPreviews),
                        System.Windows.Threading.DispatcherPriority.Background);

                    // small big preview
                    Dispatcher.Invoke(() => map.MapLargePreview = CacheService.GetImage(new(map.ThumbnailUrlLarge), Models.Folder.MapsLargePreviews),
                        System.Windows.Threading.DispatcherPriority.Background);
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

        private void OnCurrentPageMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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
