using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models.API.Base;
using beta.Models.API.MapsVault;
using beta.Models.Enums;
using beta.Models.Scmap;
using beta.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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

            if (nav is not null)
            {
                BackButtonVisibility = Visibility.Visible;
            }
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

        #region UI

        public Visibility BackButtonVisibility { get; set; } = Visibility.Collapsed;

        #endregion

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
                Data.Reviews = result.Data.Reviews.OrderByDescending(review => review.CreateTime).ToArray();
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
                    var file = App.GetPathToFolder(Folder.Maps) + model.FolderName+ '\\' + (model.FolderName.Contains('.') ?
                        model.FolderName.Split('.')[0] : model.FolderName) + ".scmap";
                    OpenMap(file);
                    break;
            }//
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
            result.Data.Reviews = result.Data.Reviews.OrderByDescending(review => review.CreateTime).ToArray();
            Data = result.Data;
        }

        private Model3DGroup _Model3DGroup;
        public Model3DGroup Model3DGroup
        {
            get => _Model3DGroup;
            set
            {
                if (Set(ref _Model3DGroup, value))
                {

                }
            }
        }
        public Visibility Model3DVisibility => Is3DEnabled ? Visibility.Visible : Visibility.Collapsed;
        private bool _Is3DEnabled;
        public bool Is3DEnabled
        {
            get => _Is3DEnabled;
            set
            {
                if (Set(ref _Is3DEnabled, value))
                {
                    OnPropertyChanged(nameof(Model3DVisibility));
                }
            }
        }

        #region Show3DModelCommand
        private ICommand _Show3DModelCommand;
        public ICommand Show3DModelCommand => _Show3DModelCommand ??= new LambdaCommand(OnShow3DModelCommand, CanShow3DModelCommand);
        public bool CanShow3DModelCommand(object p) => Model3DGroup is not null;
        public void OnShow3DModelCommand(object p) => Is3DEnabled = !Is3DEnabled;

        #endregion


        /**
		 * <summary>
		 * Method that create a water effect for the terrain
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 * <param name="waterHeightValue">water height value</param>
		 */
        private void _DrawWater(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue, float waterHeightValue)
        {
            float halfSize = terrainSize / 2;
            float halfheight = (maxHeightValue - minHeightValue) / 2;

            // creation of the water layers
            // I'm going to use a series of emissive layer for water
            SolidColorBrush waterSolidColorBrush = new SolidColorBrush(Colors.Blue);
            waterSolidColorBrush.Opacity = 0.2;
            GeometryModel3D myWaterGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new EmissiveMaterial(waterSolidColorBrush));
            Point3DCollection waterPoint3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            int triangleCounter = 0;
            float dfMul = 5;
            for (int i = 0; i < 10; i++)
            {
                triangleCounter = waterPoint3DCollection.Count;

                waterPoint3DCollection.Add(new Point3D(-halfSize, -halfSize, waterHeightValue - i * dfMul - halfheight));
                waterPoint3DCollection.Add(new Point3D(+halfSize, +halfSize, waterHeightValue - i * dfMul - halfheight));
                waterPoint3DCollection.Add(new Point3D(-halfSize, +halfSize, waterHeightValue - i * dfMul - halfheight));
                waterPoint3DCollection.Add(new Point3D(+halfSize, -halfSize, waterHeightValue - i * dfMul - halfheight));

                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 3);
                triangleIndices.Add(triangleCounter + 1);
            }

            ((MeshGeometry3D)myWaterGeometryModel.Geometry).Positions = waterPoint3DCollection;
            ((MeshGeometry3D)myWaterGeometryModel.Geometry).TriangleIndices = triangleIndices;
            Model3DGroup.Children.Add(myWaterGeometryModel);
        }

        /**
		 * <summary>
		 * Method that create the 3d terrain on a Viewport3D control
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 */
        private void _DrawTerrain(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue)
        {
            _DrawTerrain(terrainMap, terrainSize, minHeightValue, maxHeightValue, 0, 0, terrainSize, terrainSize);
        }


        /**
		 * <summary>
		 * Method that create the 3d terrain on a Viewport3D control
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 * <param name="posX">position to start the draw along x of the map</param>
		 * <param name="posY">position to start the draw along y of the map</param>
		 * <param name="maxPosX">position till which x coordinate can draw along the map</param>
		 * <param name="maxPosY">position till which y coordinate can draw along the map</param>
		 */
        private void _DrawTerrain(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue, int posX, int posY, int maxPosX, int maxPosY)
        {
            float halfSize = terrainSize / 2;
            float halfheight = (maxHeightValue - minHeightValue) / 2;

            // creation of the terrain
            GeometryModel3D myTerrainGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(new SolidColorBrush(Colors.Gray)));
            Point3DCollection point3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            //adding point
            for (var y = posY; y < maxPosY; y++)
            {
                for (var x = posX; x < maxPosX; x++)
                {
                    point3DCollection.Add(new Point3D(x - halfSize, y - halfSize, terrainMap[x, y] - halfheight));
                }
            }
            ((MeshGeometry3D)myTerrainGeometryModel.Geometry).Positions = point3DCollection;

            //defining triangles
            int ind1 = 0;
            int ind2 = 0;
            int xLenght = maxPosX - posX;
            for (var y = 0; y < maxPosY - posY - 1; y++)
            {
                for (var x = 0; x < maxPosX - posX - 1; x++)
                {
                    ind1 = x + y * (xLenght);
                    ind2 = ind1 + (xLenght);

                    //first triangle
                    triangleIndices.Add(ind1);
                    triangleIndices.Add(ind2 + 1);
                    triangleIndices.Add(ind2);

                    //second triangle
                    triangleIndices.Add(ind1);
                    triangleIndices.Add(ind1 + 1);
                    triangleIndices.Add(ind2 + 1);
                }
            }
            ((MeshGeometry3D)myTerrainGeometryModel.Geometry).TriangleIndices = triangleIndices;
            Model3DGroup.Children.Add(myTerrainGeometryModel);
        }


        /**
		 * <summary>
		 * Method that create a box container for the terrain
		 * </summary>
		 *
		 * <param name="terrainMap">terrain to show</param>
		 * <param name="terrainSize">terrain size</param>
		 * <param name="minHeightValue">minimum terraing height</param>
		 * <param name="maxHeightValue">maximum terraing height</param>
		 * <param name="waterHeightValue">water height value</param>
		 */
        private void _DrawBox(float[,] terrainMap, int terrainSize, float minHeightValue, float maxHeightValue, float waterHeightValue)
        {
            float halfSize = terrainSize / 2;
            float halfheight = (maxHeightValue - minHeightValue) / 2;

            // creation of an external box that will contain the object
            GeometryModel3D myBoxGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
            Point3DCollection boxPoint3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            // bottom layer
            boxPoint3DCollection.Add(new Point3D(-halfSize, -halfSize, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(-halfSize, +halfSize, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(+halfSize, +halfSize, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(+halfSize, -halfSize, minHeightValue - halfheight));
            triangleIndices.Add(0);
            triangleIndices.Add(1);
            triangleIndices.Add(2);
            triangleIndices.Add(3);
            triangleIndices.Add(0);
            triangleIndices.Add(2);

            // ddc layer
            boxPoint3DCollection.Add(new Point3D(-halfSize, -halfSize - 0.5, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(+halfSize, -halfSize - 0.5, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(-halfSize, -halfSize - 0.5, waterHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(+halfSize, -halfSize - 0.5, waterHeightValue - halfheight));
            triangleIndices.Add(4);
            triangleIndices.Add(5);
            triangleIndices.Add(6);
            triangleIndices.Add(6);
            triangleIndices.Add(5);
            triangleIndices.Add(7);

            boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, -halfSize, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, -halfSize, waterHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, +halfSize, minHeightValue - halfheight));
            boxPoint3DCollection.Add(new Point3D(-halfSize - 0.1, +halfSize, waterHeightValue - halfheight));
            triangleIndices.Add(8);
            triangleIndices.Add(9);
            triangleIndices.Add(10);
            triangleIndices.Add(9);
            triangleIndices.Add(11);
            triangleIndices.Add(10);

            int triangleCounter = 0;
            // layers along y = 0 and y = _Size
            for (var x = 0; x < terrainSize - 1; x++)
            {

                double valX = terrainMap[x, 0];
                double valX1 = terrainMap[x + 1, 0];
                if (valX < waterHeightValue) valX = waterHeightValue;
                if (valX1 < waterHeightValue) valX1 = waterHeightValue;

                triangleCounter = boxPoint3DCollection.Count;
                boxPoint3DCollection.Add(new Point3D(x - halfSize, -halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, -halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(x - halfSize, -halfSize, valX - halfheight));
                boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, -halfSize, valX1 - halfheight));
                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 3);

                int dy = terrainSize - 1;
                double valXdY = terrainMap[x, dy];
                double valX1dY = terrainMap[x + 1, dy];
                if (valXdY < waterHeightValue) valXdY = waterHeightValue;
                if (valX1dY < waterHeightValue) valX1dY = waterHeightValue;

                triangleCounter = boxPoint3DCollection.Count;

                boxPoint3DCollection.Add(new Point3D(x - halfSize, dy - halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(x - halfSize, dy - halfSize, valXdY - halfheight));
                boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, dy - halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(x + 1 - halfSize, dy - halfSize, valX1dY - halfheight));

                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 3);
                triangleIndices.Add(triangleCounter + 2);
            }
            // layers along x = 0 and x = _Size
            for (var y = 0; y < terrainSize - 1; y++)
            {

                double valY = terrainMap[0, y];
                double valY1 = terrainMap[0, y + 1];
                if (valY < waterHeightValue) valY = waterHeightValue;
                if (valY1 < waterHeightValue) valY1 = waterHeightValue;

                triangleCounter = boxPoint3DCollection.Count;

                boxPoint3DCollection.Add(new Point3D(-halfSize, y - halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(-halfSize, y - halfSize, valY - halfheight));
                boxPoint3DCollection.Add(new Point3D(-halfSize, y + 1 - halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(-halfSize, y + 1 - halfSize, valY1 - halfheight));

                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 3);
                triangleIndices.Add(triangleCounter + 2);

                int dx = terrainSize - 1;
                double valYdX = terrainMap[dx, y];
                double valY1dX = terrainMap[dx, y + 1];
                if (valYdX < waterHeightValue) valYdX = waterHeightValue;
                if (valY1dX < waterHeightValue) valY1dX = waterHeightValue;

                triangleCounter = boxPoint3DCollection.Count;

                boxPoint3DCollection.Add(new Point3D(dx - halfSize, y - halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(dx - halfSize, y + 1 - halfSize, minHeightValue - halfheight));
                boxPoint3DCollection.Add(new Point3D(dx - halfSize, y - halfSize, valYdX - halfheight));
                boxPoint3DCollection.Add(new Point3D(dx - halfSize, y + 1 - halfSize, valY1dX - halfheight));

                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 3);
            }
            ((MeshGeometry3D)myBoxGeometryModel.Geometry).Positions = boxPoint3DCollection;
            ((MeshGeometry3D)myBoxGeometryModel.Geometry).TriangleIndices = triangleIndices;
            Model3DGroup.Children.Add(myBoxGeometryModel);
        }


        public static int HeightmapId(int x, int y, float width) => ((int)(x + y * (width + 1)));

        public static ushort GetHeight(int x, int y, ushort[] heights, float width) => heights[HeightmapId(x, y, width)];

        private async Task OpenMap(string filename)
        {
            if (Model3DGroup is not null) return;
            Model3DGroup?.Children.Clear();
            if (string.IsNullOrEmpty(filename))
                return;
            if (!File.Exists(filename))
                return;
            DispatcherHelper.RunOnMainThread(() => Model3DGroup = new());
            //SCMAP
            ScmapBinaryReader stream = new ScmapBinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            #region Header section
            var magicWord = stream.ReadInt32();
            if (magicWord != 0x1a70614d)
            {
                //
                throw new System.Exception();
            }
            //? always 2
            var VersionMajor = stream.ReadInt32();
            //? always EDFE EFBE
            var unknown10 = stream.ReadInt32();
            //? always 2
            var unknown11 = stream.ReadInt32();
            var mapWidth = stream.ReadSingle();
            var mapHeight = stream.ReadSingle();
            //? always 0
            var Unknown12 = stream.ReadInt32();
            //? always 0
            var Unknown13 = stream.ReadInt16();
            int previewImageLength = stream.ReadInt32();
            var previewData = stream.ReadBytes(previewImageLength);

            var VersionMinor = stream.ReadInt32();
            #endregion
            #region Heightmap section
            var width = stream.ReadInt32();
            var height = stream.ReadInt32();
            //Height Scale, usually 1/128
            var heightScale = stream.ReadSingle();
            //heightmap dimension is always 1 more than texture dimension!
            //var heightmapData = stream.ReadInt16Array((int)((Height + 1) * (Width + 1)));
            var len = (int)((width + 1) * (height + 1));

            ushort[] g = new ushort[len];
            for (int i = 0; i < len; i++)
            {
                g[i] = (ushort)stream.ReadInt16(); ;
            }
            #region werf

            float[,] heights = new float[height, width];
            float HeightWidthMultiply = (height / (float)width);
            for (int yy = 0; yy < height; yy++)
            {
                for (int xx = 0; xx < height; xx++)
                {
                    var localY = (int)(((height - 1) - yy) * HeightWidthMultiply);
                    heights[yy, xx] = GetHeight(localY, xx, g, width) * heightScale;

                    //if (HeightWidthMultiply == 0.5f && y > 0 && y % 2f == 0)
                    //{
                    //	heights[y - 1, x] = Lerp(heights[y, x], heights[y - 2, x], 0.5f);
                    //}
                }
            }

            DispatcherHelper.RunOnMainThread(() =>
            {
                PointLight[] lights = new PointLight[]
                {
                new PointLight(Colors.White, new Point3D(0, 0, width * 3)),
                    //new PointLight(Colors.White, new Point3D(width, 0, -width * 3 / 5)),
                    //new PointLight(Colors.White, new Point3D(-width, 0, -width * 3 / 5)),
                    //new PointLight(Colors.White, new Point3D(0, height, -width * 3 / 5)),
                    //new PointLight(Colors.White, new Point3D(0, -height, -width * 3 / 5)),
                };

                foreach (var light in lights)
                {
                    Model3DGroup.Children.Add(light);
                }
                _DrawTerrain(heights, height, 0, 0);
                _DrawBox(heights, height, 0, 0, 0);
            });
            #endregion


            if (VersionMinor >= 56)
                //Always 0?
                stream.ReadByte();

            #endregion
            #region Texture section
            //Terrain Shader, usually "TTerrain"
            var TerrainShader = stream.ReadStringNull();
            var TexPathBackground = stream.ReadStringNull();
            var TexPathSkyCubemap = stream.ReadStringNull();
            #endregion
            #region Env Cubes
            ScmapEnvCube[] envCubes;
            if (VersionMinor >= 56)
            {
                var count = stream.ReadInt32();
                envCubes = new ScmapEnvCube[count];
                for (int i = 0; i < count; i++)
                {
                    envCubes[0] = new ScmapEnvCube(stream.ReadStringNull(), stream.ReadStringNull());
                }
            }
            else
            {
                envCubes = new ScmapEnvCube[]
                {
                    new ScmapEnvCube(stream.ReadStringNull(),stream.ReadStringNull()),
                    new ScmapEnvCube(stream.ReadStringNull(),stream.ReadStringNull())
                };
            }
            #endregion
            #region Settings
            var LightingMultiplier = stream.ReadSingle();
            var SunDirection = stream.ReadVector3();
            var SunAmbience = stream.ReadVector3();
            var SunColor = stream.ReadVector3();
            var ShadowFillColor = stream.ReadVector3();
            var SpecularColor = stream.ReadVector4();
            var Bloom = stream.ReadSingle();
            var FogColor = stream.ReadVector3();
            var FogStart = stream.ReadSingle();
            var FogEnd = stream.ReadSingle();
            #endregion
            #region MinimapData
            stream.Close();
            #endregion



            //var PreviewTex = Texture.FromMemory(new Device, previewData, 256, 256, 1, Usage.None, Format.A8R8G8B8, Pool.Scratch, Filter.None, Filter.None, 0);
            //var PreviewBitmap = TextureToBitmap(PreviewTex);


            //WriteableBitmap wBmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            //int bpp = PixelFormats.Bgra32.BitsPerPixel / 8;
            ////byte[] pixels = new byte[wBmp.PixelWidth * wBmp.PixelHeight];
            //Int32Rect drawRegionRect = new Int32Rect(0, 0, wBmp.PixelWidth, wBmp.PixelHeight);
            ////for (int i = 0; i < br.BaseStream.Length; ++i)
            ////	pixels[i] = br.ReadByte();
            ////br.Close();

            //int stride = wBmp.PixelWidth * bpp;
            //wBmp.WritePixels(drawRegionRect, previewData, stride, 0);
            //PngBitmapEncoder encoder5 = new PngBitmapEncoder();
            //encoder5.Frames.Add(BitmapFrame.Create(wBmp));
            //encoder5.Save(new FileStream("test.png", FileMode.CreateNew));
            ////RawFile = new CRAW(new Size(width, height), format, stride, wBmp);
        }
    }
}
