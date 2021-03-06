using beta.Models.API;
using beta.Models.API.Base;
using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace beta.ViewModels
{
    public static class ApiRequest<T> where T : class
    {
        private static readonly HttpClient HttpClient = new(); 
        public static async Task<T> RequestWithId(string url, int id, string query = null)
        {
            if (url[^1] != '/') url += '/';

            if (query is not null && query.Length > 0 && query[0] != '?') query = '?' + query;

            var requestUrl = url + id + query;

            //WebRequest request = WebRequest.Create(requestUrl);
            //request.Headers.Add("Accept", "application/json");
            //var response = await request.GetResponseAsync();
            //var response = await HttpClient.GetStreamAsync(requestUrl);
            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(await HttpClient.GetStreamAsync(requestUrl));
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<T> Request(string requestUrl)
        {
            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(await HttpClient.GetStreamAsync(requestUrl));
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    internal class ProfileViewModel : Base.ViewModel, ISelectedPlayerProfile
    {
        private readonly NavigationService NavigationService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public ProfileViewModel(int id)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        public ProfileViewModel(PlayerInfoMessage player)
        {
            Player = player;
            Task.Run(() => UpdateInfo());
        }
        public ProfileViewModel(PlayerInfoMessage player, NavigationService nav) : this(player) => 
            NavigationService = nav;

        public PlayerInfoMessage Player { get; private set; }

        #region AvatarsViewModel
        private AvatarsViewModel _AvatarsViewModel;
        public AvatarsViewModel AvatarsViewModel
        {
            get => _AvatarsViewModel;
            set => Set(ref _AvatarsViewModel, value);
        }
        #endregion

        #region ApiRatingsViewModel
        private ApiRatingsViewModel _ApiRatingsViewModel;
        public ApiRatingsViewModel ApiRatingsViewModel
        {
            get => _ApiRatingsViewModel;
            set => Set(ref _ApiRatingsViewModel, value);
        }
        #endregion

        #region ApiNameRecordsViewModel
        private ApiNameRecordsViewModel _ApiNameRecordsViewModel;
        public ApiNameRecordsViewModel ApiNameRecordsViewModel
        {
            get => _ApiNameRecordsViewModel;
            set => Set(ref _ApiNameRecordsViewModel, value);
        }
        #endregion

        #region ApiPlayerData
        private ApiPlayerData _ApiPlayerData;
        public ApiPlayerData ApiPlayerData
        {
            get => _ApiPlayerData;
            set
            {
                if (Set(ref _ApiPlayerData, value))
                {
                    if (value is not null)
                    {
                        if (value.Avatars is not null && value.Avatars.Data.Count > 0)
                        {
                            AvatarsViewModel = new(Player.id, value.Avatars.Data
                                .Select(x=>x.Id)
                                .ToArray());
                        }
                        if (value.Names is not null && value.Names.Data.Count > 0)
                        {
                            ApiNameRecordsViewModel = new(Player.id);
                        }
                    }
                }
            }
        }
        #endregion

        public async Task UpdateInfo()
        {
            var result = await ApiRequest<ApiUniversalResult<ApiPlayerData>>.RequestWithId("https://api.faforever.com/data/player/", Player.id);
            ApiPlayerData = result.Data;
            ApiRatingsViewModel = new ApiRatingsViewModel(Player.id, Player.ratings.Keys.ToArray());
        }
    }
}
