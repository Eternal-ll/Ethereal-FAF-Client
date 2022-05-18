using beta.Models.API.Base;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    internal class MapViewModel : ApiViewModel
    {
        public MapViewModel(int id)
        {

        }
        public MapViewModel(string name) => 
            Task.Run(() => GetMapId(name));

        private async Task GetMapId(string name)
        {
            IsPendingRequest = true;

            string api = "https://api.faforever.com/data/map";

            bool hasVersion = name.Contains('.');

            IsPendingRequest = false;
        }

        protected override async Task RequestTask()
        {

        }
    }

    /// <summary>
    /// API view model for mapVersion entity
    /// </summary>
    internal class MapVersionViewModel : ApiViewModel
    {
        public MapVersionViewModel(int id) => Id = id;
        public MapVersionViewModel(string name) =>
            Task.Run(() => GetMapVersionId(name));

        #region Properties

        #region Data - API result
        private ApiUniversalData _Data;
        public ApiUniversalData Data
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
            var request = WebRequest.Create(api + query);
            var response = await request.GetResponseAsync();
            var result = await JsonSerializer.DeserializeAsync<ApiUniversalTypeId>(response.GetResponseStream());
            Id = result.Id;
        }

        protected override async Task RequestTask()
        {
            string api = "https://api.faforever.com/data/mapVersion";
            string query = $"?filter=(id=={Id})";
            var request = WebRequest.Create(api + query);
            var response = await request.GetResponseAsync();
            var result = await JsonSerializer.DeserializeAsync<ApiUniversalData>(response.GetResponseStream());
            Data = result;
        }
    }
}
