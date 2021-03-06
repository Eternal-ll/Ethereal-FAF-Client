using beta.Models.API;
using beta.Models.API.Base;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    internal class ApiNameRecordsViewModel : ApiPlayerViewModel
    {
        public ApiNameRecordsViewModel(int playerId) : base(playerId) 
        {
            RunRequest();
        }

        #region Records
        private ApiPlayerNameRecord[] _Records;
        public ApiPlayerNameRecord[] Records
        {
            get => _Records;
            set => Set(ref _Records, value);
        }
        #endregion

        protected override async Task RequestTask()
        {
            string url = $"https://api.faforever.com/data/nameRecord?filter=(player.id=={PlayerId})";
            var result = await ApiRequest<ApiUniversalResult<ApiPlayerNameRecord[]>>.Request(url);
            Records = result.Data;
        }
    }
}
