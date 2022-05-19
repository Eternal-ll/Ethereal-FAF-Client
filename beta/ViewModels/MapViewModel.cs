using beta.Models.API.Base;
using beta.Models.API.MapsVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    internal class MapViewModel : ApiViewModel
    {
        private readonly ILogger Logger;

        public MapViewModel()
        {
            Logger = App.Services.GetService<ILogger<MapViewModel>>();
        }
        public MapViewModel(int id) : this() => Id = id;
        public MapViewModel(string name) : this() => 
            Task.Run(() => GetMapId(name));

        private async Task GetMapId(string name)
        {
            IsPendingRequest = true;

            bool hasVersion = name.Contains('.');
            if (hasVersion) name = name.Split('.')[0];
            string query = $"https://api.faforever.com/data/map?filter=(displayName==\"{name}\")";

            IsPendingRequest = false;
        }

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
            set => Set(ref _CurrentMapVersionVM, value);
        }

        #endregion

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

            if (result.Data.Versions is not null & result.Data.Versions.Length > 0)
            {
                Versions = new();
                foreach (var entity in result.Data.Versions)
                {
                    Versions.Add(entity.Id, null);
                }
                SelectedMapVersion = result.Data.Versions[^1];
            }
            Data = result.Data;
        }
    }

    /// <summary>
    /// API view model for mapVersion entity
    /// </summary>
    internal class MapVersionViewModel : ApiViewModel
    {
        private readonly ILogger Logger;

        public MapVersionViewModel()
        {
            Logger = App.Services.GetService<ILogger<MapViewModel>>();
        }
        public MapVersionViewModel(int id) : this() => Id = id;
        public MapVersionViewModel(string name) : this() =>
            Task.Run(() => GetMapVersionId(name));

        #region Properties

        #region Data - API result
        private MapVersionModel _Data;
        public MapVersionModel Data
        {
            get => _Data;
            set => Set(ref _Data, value);
        }
        #endregion

        #endregion


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

        protected override async Task RequestTask()
        {
            //var playersData = await ApiRequest<ApiUniversalResult<ApiUniversalData>>.RequestWithId(
            //    url: "https://api.faforever.com/data/mapVersion/",
            //    id: Id,
            //    query: "include=reviews.player&fields[player]=login");
            

            var result = await ApiRequest<ApiMapVersionResult>.RequestWithId(
                url: "https://api.faforever.com/data/mapVersion/",
                id: Id,
                query: "?include=reviews,reviewsSummary,statistics,reviews.player");
                //"&fields[mapVersion]=createTime,description,downloadUrl,filename,folderName,recommended,updateTime" +
                //"&fields[mapVersionStatistics]=downloads,draws,plays" +
                //"&fields[mapVersionReview]=createTime,score,text,updateTime" +
                //"&fields[mapVersionReviewsSummary]=lowerBound,negative,positive,reviews,score");
            result.ParseIncluded();
            Data = result.Data;
        }
    }
}
