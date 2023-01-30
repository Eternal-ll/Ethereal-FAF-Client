using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class ServersManagement : ViewModel
    {
        public event EventHandler<ServerManager> ServerManagerAdded;
        public event EventHandler<ServerManager> ServerManagerRemoved;
        public event EventHandler<(Server server, string OAuthUrl)> OAuthRequired;
        public event EventHandler<Server> OAuthUrlExpired;
        public event EventHandler<(Server server, string error, string description)> ErrorOccuried;

        private readonly IServiceProvider ServiceProvider;
        private readonly IConfiguration Configuration;
        private readonly ILogger Logger;

        public ObservableCollection<ServerManager> ServersManagers { get; set; }

        #region SelectedServerManager
        private ServerManager _SelectedServerManager;
        /// <summary>
        /// Selected server
        /// </summary>
        public ServerManager SelectedServerManager
        {
            get => _SelectedServerManager;
            set => Set(ref _SelectedServerManager, value);
        } 
        #endregion
        public ObservableCollection<Server> Servers { get; }

        public ServersManagement(IServiceProvider serviceProvider, IConfiguration configuration, IHttpClientFactory httpClientFactory,
            ILogger<ServersManagement> logger)
        {
            ServiceProvider = serviceProvider;
            Configuration = configuration;
            Logger = logger;

            ServersManagers = new();

            var servers = Configuration.GetSection("Servers").Get<List<Server>>();

            var gapforeverAddresses = Array.Empty<string>();
            try
            {
                gapforeverAddresses = Dns.GetHostEntry("gapforever.com").AddressList.Select(p => p.ToString()).ToArray();
            }
            catch
            {

            }

            var client = httpClientFactory.CreateClient();

            var newServers = new List<Server>();

            foreach (var server in servers)
            {
                if (server.Lobby is null) continue;
                if (server.Lobby.Host.Contains("gap", StringComparison.OrdinalIgnoreCase) ||
                    server.Api.Host.Contains("gap", StringComparison.OrdinalIgnoreCase) == true ||
                    server.Irc.Host.Contains("gap", StringComparison.OrdinalIgnoreCase) ||
                    gapforeverAddresses.Contains(server.Lobby.Host))
                {
                    servers.Remove(server);
                    continue;
                }
                newServers.Add(server);
                if (!string.IsNullOrWhiteSpace(server.ConfigurationFile))
                {
                    DownlordClientConfiguration downlordConfiguration = null!;
                    try
                    {
                        downlordConfiguration = client.GetFromJsonAsync<DownlordClientConfiguration>(server.Content.ToString() + server.ConfigurationFile).Result;
                    }
                    catch
                    {
                        logger.LogWarning("Failed to get Downlord`s client configuration from [{host}]", server.Content.ToString() + server.ConfigurationFile);
                        continue;
                    }
                    foreach (var endpoint in downlordConfiguration.Endpoints.Where(e => e.Lobby.Host != server.Lobby.Host))
                    {
                        var serverFromConfig = new Server()
                        {
                            Name = $"{endpoint.Name}",
                            ShortName = $"{endpoint.Name}",
                            //Site = server.Site,
                            Cloudfare = server.Cloudfare,
                            Content = server.Content,
                            Lobby = new()
                            {
                                Host = endpoint.Lobby.Host,
                                Port = 8002
                            },
                            Irc = new()
                            {
                                Host = endpoint.Irc.Host,
                                Port = endpoint.Irc.Port
                            },
                            Api = endpoint.Api.Url,
                            OAuth = new()
                            {
                                BaseAddress = endpoint.Oauth.Url,
                                ClientId = "faf-java-client",
                                RedirectPorts = server.OAuth.RedirectPorts,
                                ResponseSeconds = server.OAuth.ResponseSeconds,
                                Scope = server.OAuth.Scope
                            },
                            Logo = server.Logo,
                            Relay = new()
                            {
                                Host = endpoint.Lobby.Host,
                                Port = server.Relay.Port
                            },
                            Replay = new()
                            {
                                Host = endpoint.LiveReplay.Host,
                                Port = endpoint.LiveReplay.Port
                            },
                            IsVisible = true,
                        };
                        newServers.Add(serverFromConfig);
                    }
                }
            }

            Servers = new(newServers);
        }

        public async Task AuthorizeAndConnectToServerAsync(Server server, CancellationToken cancellationToken)
        {
            var serverManager = ServiceProvider.GetRequiredService<ServerManager>();
            serverManager.GetOAuthProvider().OAuthRequired += ServersManagement_OAuthRequired;
            serverManager.GetOAuthProvider().OAuthUrlExpired += ServersManagement_OAuthUrlExpired;
            serverManager.GetOAuthProvider().ErrorOccuried += ServersManagement_ErrorOccuried;
            serverManager.SetServer(server);
            await serverManager.AuthorizeAndConnectAsync(server, cancellationToken);
            ServersManagers.Add(serverManager);
            SelectedServerManager = serverManager;
            ServerManagerAdded?.Invoke(this, serverManager);

            serverManager.GetOAuthProvider().OAuthRequired -= ServersManagement_OAuthRequired;
            serverManager.GetOAuthProvider().OAuthUrlExpired -= ServersManagement_OAuthUrlExpired;
            serverManager.GetOAuthProvider().ErrorOccuried -= ServersManagement_ErrorOccuried;
        }

        private void ServersManagement_ErrorOccuried(object sender, (Server server, string error, string description) e)
        {
            ErrorOccuried?.Invoke(sender, e);
        }

        private void ServersManagement_OAuthUrlExpired(object sender, Server e)
        {
            OAuthUrlExpired?.Invoke(this, e);
        }

        private void ServersManagement_OAuthRequired(object sender, (Server server, string OAuthUrl) e)
        {
            OAuthRequired?.Invoke(sender, e);
        }

        public async Task DisconnectedFromServer(ServerManager serverManager)
        {
            ServerManagerRemoved?.Invoke(this, serverManager);
        }
    }
}
