using beta.Models.API;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    public class ApiClientUsersCounterViewModel : ApiViewModel
    {
        public ApiClientUsersCounterViewModel()
        {
            RunRequest();
        }

        #region ClientCounterDictionary
        private Dictionary<string, int> _ClientCounterDictionary;
        public Dictionary<string, int> ClientCounterDictionary
        {
            get => _ClientCounterDictionary;
            set => Set(ref _ClientCounterDictionary, value);
        }
        #endregion

        #region SelectedDate
        private DateTime _SelectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _SelectedDate;
            set
            {
                if (Set(ref _SelectedDate, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

        private string BuildQuery()
        {
            StringBuilder sb = new();

            sb.Append($"&filter=(updateTime=gt=\"{SelectedDate:yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'}\")");

            return sb.ToString();
        }

        protected override async Task RequestTask()
        {
            string query = BuildQuery();
            string url = "https://api.faforever.com/data/player?&fields[player]=userAgent,updateTime&page[totals]=true";
            WebRequest request = WebRequest.Create(url + query);
            var result = await JsonSerializer.DeserializeAsync<ApiUniversalResultWithMeta<ApiPlayerData[]>>(request.GetResponse().GetResponseStream());
            Dictionary<string, int> dic = new();
            var pagesCount = result.Meta.Page.AvaiablePagesCount;
            for (int i = 1; i <= pagesCount; i++)
            {
                request = WebRequest.Create(url + query + $"&page[number]={i}");
                result = await JsonSerializer.DeserializeAsync<ApiUniversalResultWithMeta<ApiPlayerData[]>>(request.GetResponse().GetResponseStream());
                var players = result.Data;
                foreach (var player in players)
                {
                    if (dic.ContainsKey(player.UserAgent))
                    {
                        dic[player.UserAgent]++;
                    }
                    else
                    {
                        dic.Add(player.UserAgent, 1);
                    }
                }
            }

            ClientCounterDictionary = dic;
        }
    }
}
