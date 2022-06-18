using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models.API.Base;
using beta.Models.API.MapsVault;
using beta.Models.Enums;
using beta.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace beta.ViewModels
{
    internal class MapViewModel : ApiViewModel
    {
        private readonly ILogger Logger;
        private readonly IMapsService MapsService;
        private readonly IDownloadService DownloadService;
        private readonly NavigationService NavigationService;

        public MapViewModel(NavigationService nav = null)
        {
            Logger = App.Services.GetService<ILogger<MapViewModel>>();
            MapsService = App.Services.GetService<IMapsService>();
            DownloadService = App.Services.GetService<IDownloadService>();
            NavigationService = nav;
        }
        public MapViewModel(int id, NavigationService nav = null) : this(nav) => Id = id;
        public MapViewModel(string name, NavigationService nav = null) : this(nav) => 
            Task.Run(() => GetMapId(name));
        public MapViewModel(ApiMapModel selected, ApiMapModel[] similar, NavigationService nav = null) : this(nav)
        {
            IsPendingRequest = true;
            Id = selected.Id;
            IsPendingRequest = false;
            Data = selected;
            if (similar is not null && similar.Length == 0) similar = null;
            Similar = similar;
            Task.Run(async () =>
            {
                var result = await ApiRequest<ApiMapResult>.RequestWithId(
                    url: "https://api.faforever.com/data/map/",
                    id: Id,
                    query: "?include=versions" +
                    "&fields[mapVersion]=version");
                result.ParseIncluded();
                Data.Versions = result.Data.Versions;
                Versions = new();
                VersionValues = new();
                foreach (var entity in result.Data.Versions)
                {
                    VersionValues.Add(entity.Id);
                    if (selected.LatestVersion.Id == entity.Id)
                    {
                        Versions.Add(entity.Id, new(selected.LatestVersion));
                        SelectedMapVersion = entity;
                        //DispatcherHelper.RunOnMainThread(() => OnPropertyChanged(nameof(SelectedMapVersion)));
                    }
                    else
                    {
                        Versions.Add(entity.Id, null);
                    }
                }
            });
        }

        private async Task GetMapId(string name)
        {
            IsPendingRequest = true;

            bool hasVersion = name.Contains('.');
            if (hasVersion) name = name.Split('.')[0];
            string query = $"https://api.faforever.com/data/map?filter=(displayName==\"{name}\")";

            IsPendingRequest = false;
        }

        public ApiMapModel[] Similar { get; set; }

        #region Data
        private ApiMapModel _Data;
        public ApiMapModel Data
        {
            get => _Data;
            set => Set(ref _Data, value);
        }
        #endregion

        private Dictionary<int, MapVersionViewModel> Versions;

        #region SelectedMapVersion
        private MapVersionModel _SelectedMapVersion;
        public MapVersionModel SelectedMapVersion
        {
            get => _SelectedMapVersion;
            set
            {
                if (Set(ref _SelectedMapVersion, value))
                {
                    if (value is null)
                    {
                        CurrentMapVersionVM = null;
                    }
                    else
                    {
                        if (Versions.TryGetValue(value.Id, out var vm) && vm is not null)
                        {
                            CurrentMapVersionVM = vm;
                        }
                        else
                        {
                            CurrentMapVersionVM = new(value.Id);
                            Versions[value.Id] = CurrentMapVersionVM;
                        }
                    }
                }
            }
        }
        #endregion

        #region CurrentMapVersionVM
        private MapVersionViewModel _CurrentMapVersionVM;
        public MapVersionViewModel CurrentMapVersionVM
        {
            get => _CurrentMapVersionVM;
            set
            {
                if (Set(ref _CurrentMapVersionVM, value))
                {
                    if (value is not null && value.Data is null)
                    {
                        value.RequestFinished += Value_RequestFinished;
                    }
                    else
                    {
                        OnPropertyChanged(nameof(ButtonsVisibility));
                    }
                }
            }
        }

        private void Value_RequestFinished(object sender, System.EventArgs e)
        {
            OnPropertyChanged(nameof(ButtonsVisibility));
        }
        #endregion

        #region DownloadsModel
        private DownloadViewModel _DownloadsModel;
        public DownloadViewModel DownloadsModel
        {
            get => _DownloadsModel;
            set => Set(ref _DownloadsModel, value);
        }
        #endregion

        public Visibility ButtonsVisibility
        {
            get
            {
                //if (CurrentMapVersionVM?.Data?.IsHidden is true &&
                //    CurrentMapVersionVM?.DeleteVisibility == Visibility.Collapsed) return Visibility.Collapsed;

                return
                CurrentMapVersionVM?.InstalledVisibility == Visibility.Visible ||
                CurrentMapVersionVM?.DownloadVisibility == Visibility.Visible ||
                CurrentMapVersionVM?.UpdateVisibility == Visibility.Visible ||
                CurrentMapVersionVM?.DeleteVisibility == Visibility.Visible ?
                Visibility.Visible : Visibility.Collapsed;
            }
        }

        private List<int> VersionValues;

        protected override async Task RequestTask()
        {
            var result = await ApiRequest<ApiMapResult>.RequestWithId(
                url: "https://api.faforever.com/data/map/",
                id: Id,
                query: "?include=author,reviewsSummary,statistics,versions" +
                "&fields[mapVersion]=version" +
                "&fields[player]=login" +
                "&fields[mapStatistics]=downloads,draws,plays" +
                "&fields[map]=battleType,createTime,displayName,gamesPlayed,mapType,recommended,updateTime" +
                "&fields[mapReviewsSummary]=lowerBound,negative,positive,reviews,score");
            result.ParseIncluded();

            if (result.Data.Versions is not null & result.Data.Versions.Length > 0 && Versions is null)
            {
                Versions = new();
                VersionValues = new();
                foreach (var entity in result.Data.Versions)
                {
                    Versions.Add(entity.Id, null);
                    VersionValues.Add(entity.Id);
                }
                result.Data.Versions[^1].IsLatestVersion = true;
                SelectedMapVersion = result.Data.Versions[^1];
            }
            Data = result.Data;
        }

        #region BackCommand
        private ICommand _BackCommand;
        public ICommand BackCommand => _BackCommand ??= new LambdaCommand(OnBackCommand);
        private void OnBackCommand(object parameter) => NavigationService?.GoBack();
        #endregion

        #region OpenDetailsCommand
        private ICommand _OpenDetailsCommand;
        public ICommand OpenDetailsCommand => _OpenDetailsCommand ??= new LambdaCommand(OnOpenDetailsCommand);
        private void OnOpenDetailsCommand(object parameter)
        {
            if (parameter is null) return;
            if (parameter is int id)
            {
                var maps = Similar;
                var selected = maps.First(m => m.Id == id);
                NavigationService?.Navigate(new MapDetailsView(selected, 
                    maps
                    .Where(m => m.Id != id)
                    .ToArray(), NavigationService));
            }
        }
        #endregion
    }

    /// <summary>
    /// API view model for mapVersion entity
    /// </summary>
    internal class MapVersionViewModel : ApiViewModel
    {
        private readonly ILogger Logger;
        private readonly IMapsService MapsService;

        public MapVersionViewModel()
        {
            Logger = App.Services.GetService<ILogger<MapViewModel>>();
            MapsService = App.Services.GetService<IMapsService>();
        }
        public MapVersionViewModel(int id) : this() => Id = id;
        public MapVersionViewModel(string name) : this() =>
            Task.Run(() => GetMapVersionId(name));
        public MapVersionViewModel(MapVersionModel version) : this()
        {
            IsPendingRequest = true;
            Id = version.Id;
            HandleLocalState(version);
            Data = version;
            Task.Run(async () =>
            {
                var result = await ApiRequest<ApiMapVersionResult>.RequestWithId(
                    url: "https://api.faforever.com/data/mapVersion/",
                    id: Id,
                    query: "?include=reviews,reviewsSummary,statistics&fields[mapVersionReview]=createTime,score,text,updateTime&fields[mapVersionReviewsSummary]=lowerBound,negative,positive,reviews,score&fields[mapVersionStatistics]=plays");
                result.ParseIncluded();
                Data.Reviews = result.Data.Reviews;
                Data.Summary = result.Data.Summary;
                Data.Statistics = result.Data.Statistics;
                DispatcherHelper.RunOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(Data.Reviews));
                    OnPropertyChanged(nameof(Data.Summary));
                    OnPropertyChanged(nameof(Data.Statistics));
                });
            });
            IsPendingRequest = false;
        }

        #region Properties

        #region Data - API result
        private MapVersionModel _Data;
        public MapVersionModel Data
        {
            get => _Data;
            set
            {
                if (Set(ref _Data, value))
                {
                    OnPropertyChanged(nameof(DownloadVisibility));
                    OnPropertyChanged(nameof(DeleteVisibility));
                    OnPropertyChanged(nameof(UpdateVisibility));
                    OnPropertyChanged(nameof(InstalledVisibility));

                    OnPropertyChanged(nameof(PlugVisibility));
                }
            }
        }
        #endregion

        public Visibility LatestVisibility =>
            Data?.IsLatestVersion is true &&
            Data?.IsHidden is false ?
            Visibility.Visible :
            Visibility.Collapsed;

        public Visibility DownloadVisibility =>
            Data?.LocalState == LocalMapState.NotExist &&
            Data?.IsHidden is false ?
            Visibility.Visible :
            Visibility.Collapsed;
        public Visibility UpdateVisibility =>
            Data?.LocalState == LocalMapState.Newest ?
            Visibility.Visible :
            Visibility.Collapsed;
        public Visibility DeleteVisibility =>
            Data?.LocalState == LocalMapState.Same &&
            Data?.IsLegacyMap is false ?
            Visibility.Visible :
            Visibility.Collapsed;
        public Visibility InstalledVisibility =>
            Data?.LocalState == LocalMapState.Same ?
            Visibility.Visible :
            Visibility.Collapsed;

        public Visibility PlugVisibility =>
            Data?.IsHidden is true ? Visibility.Visible : Visibility.Collapsed;

        #endregion

        #region DownloadsVisibility
        private Visibility _DownloadsVisibility = Visibility.Collapsed;
        public Visibility DownloadsVisibility
        {
            get => _DownloadsVisibility;
            set => Set(ref _DownloadsVisibility, value);
        }
        #endregion

        #region DeleteCommand
        private ICommand _DeleteCommand;
        public ICommand DeleteCommand => _DeleteCommand ?? new LambdaCommand(OnDeleteCommand, CanDeleteCommand);
        private bool CanDeleteCommand(object parameter) => true;
        private void OnDeleteCommand(object parameter)
        {
            if (Data?.LocalState is not LocalMapState.Same) return;
            MapsService.Delete(Data.FolderName);
            HandleLocalState(Data);
        }
        #endregion

        #region DownloadCommand
        private ICommand _DownloadCommand;
        public ICommand DownloadCommand => _DownloadCommand ??= new LambdaCommand(OnDownloadCommand, CanDownloadCommand);
        private bool CanDownloadCommand(object parameter) => true;
        private void OnDownloadCommand(object parameter)
        {
            if (Data?.DownloadUrl is null) return;
            Task.Run(() => DownloadAndExtract())
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {

                    }
                });
        }
        #endregion
        private async Task DownloadAndExtract()
        {
            DownloadsVisibility = Visibility.Visible;
            var model = await MapsService.DownloadAndExtractAsync(Data.DownloadUrl, false);
            DownloadsVisibility = Visibility.Collapsed;
            if (model.IsCompleted)
            {
                HandleLocalState(Data);
            }
        }


        private async Task GetMapVersionId(string name)
        {
            IsPendingRequest = true;
            string api = "https://api.faforever.com/data/mapVersion";
            bool hasVersion = name.Contains('.');
            string query = $"?filter=(folderName==\"{(hasVersion ? name : name + ".*")}\")&fields[mapVersion]";
            if (hasVersion) query += "&sort=-id";
            var request = WebRequest.Create(api + query);
            var response = await request.GetResponseAsync();
            var result = await JsonSerializer.DeserializeAsync<ApiUniversalTypeId>(response.GetResponseStream());
            Id = result.Id;
        }

        private void HandleLocalState(MapVersionModel model)
        {
            var localState = MapsService.CheckLocalMap(model.FolderName);
            model.LocalState = localState;
            OnPropertyChanged(nameof(model.LocalState));
            switch (localState)
            {
                case LocalMapState.Older:
                case LocalMapState.Newest:
                case LocalMapState.Same:
                    model.Scenario = MapsService.GetMapScenario(model.FolderName, model.IsLegacyMap);
                    OnPropertyChanged(nameof(model.Scenario));
                    break;
            }
            OnPropertyChanged(nameof(DownloadVisibility));
            OnPropertyChanged(nameof(DeleteVisibility));
            OnPropertyChanged(nameof(UpdateVisibility));
            OnPropertyChanged(nameof(InstalledVisibility));
        }

        protected override async Task RequestTask()
        {
            var result = await ApiRequest<ApiMapVersionResult>.RequestWithId(
                url: "https://api.faforever.com/data/mapVersion/",
                id: Id,
                query: "?include=reviews,reviewsSummary,statistics,reviews.player");
                //"&fields[mapVersion]=createTime,description,downloadUrl,filename,folderName,recommended,updateTime" +
                //"&fields[mapVersionStatistics]=downloads,draws,plays" +
                //"&fields[mapVersionReview]=createTime,score,text,updateTime" +
                //"&fields[mapVersionReviewsSummary]=lowerBound,negative,positive,reviews,score");
            result.ParseIncluded();

            if (string.IsNullOrWhiteSpace(result.Data.Description))
                result.Data.Attributes["description"] = "No description";
            else
            {
                result.Data.Attributes["description"] = result.Data.Description
                    .Replace("\\r", "\r")
                    .Replace("\\n", "\n");
            }

            if (result.Data.IsLegacyMap)
            {
                result.Data.Attributes["hidden"] = "false";
            }

            HandleLocalState(result.Data);

            Data = result.Data;
        }
    }
}
