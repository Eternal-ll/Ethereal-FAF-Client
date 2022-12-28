using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.ViewModels.Servers
{
    public class SelectServerVM : Base.ViewModel
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory HttpClientFactory;

        public SelectServerVM(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            Configuration = configuration;
            HttpClientFactory = httpClientFactory;

            Servers = new(Configuration.GetSection("Servers").Get<Server[]>());
            var client = HttpClientFactory.CreateClient();
            ApiTimer = new(async (obj) =>
            {
                foreach (var server in Servers)
                {
                    client.BaseAddress = server.Site.Append("/lobby_api");
                    try
                    {
                        server.SetPlayersCount(int.Parse(await client.GetStringAsync("?resource=players")));
                        server.SetGamesCount(int.Parse(await client.GetStringAsync("?resource=games")));
                    }
                    catch
                    {

                    }
                }
            }, null, 5000, 15000);
        }

        private Timer ApiTimer;

        public ObservableCollection<Server> Servers { get; }
        #region SelectedServer
        private Server _SelectedServer;
        public Server SelectedServer
        {
            get => _SelectedServer;
            set => Set(ref _SelectedServer, value);
        }
        #endregion
    }
}
