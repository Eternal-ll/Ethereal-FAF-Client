using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public class FafOAuthClient
    {
        public event EventHandler<string> OAuthLinkGenerated;

        private readonly string ClientId;
        private readonly string Scope;
        private readonly int[] RedirectPorts;
        private readonly string BaseAddress;
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly ILogger Logger;

        public FafOAuthClient(string clientId, string scope, int[] redirectPorts, string baseAddress, IHttpClientFactory httpClientFactory, ILogger<FafOAuthClient> logger)
        {
            ClientId = clientId;
            Scope = scope;
            RedirectPorts = redirectPorts;
            BaseAddress = baseAddress;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
            logger.LogTrace("Initialized with client id [{clientId}], scope [{scope}], redirect ports [{redirect}]", clientId, scope, @redirectPorts);
        }
        public async Task<OAuthResult> RefreshToken(string refreshToken, CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            progress?.Report("Refreshing access token");
            var result = new OAuthResult()
            {
                ErrorDescription = "Something went wrong"
            };
            var token = await FetchOAuthToken(refreshToken, true, 0, cancellationToken);
            if (token is not null)
            {
                progress?.Report("Access token refreshed");
                result.IsError = false;
                result.TokenBearer = token;
            }
            progress?.Report("Something went wrong");
            return result;
        }

        public async Task<OAuthResult> AuthByBrowser(CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var result = new OAuthResult();
            Logger.LogTrace("Processing OAuth authorization by browser");
            using var httpListener = new HttpListener();
            var ports = RedirectPorts;
            var freePort = 0;

            Logger.LogTrace("Searching free ports for redirect in given range [{ports}]", ports);
            foreach (var port in ports)
            {
                try
                {
                    Logger.LogTrace("Process port [{port}]", port);
                    httpListener.Prefixes.Clear();
                    httpListener.Prefixes.Add($"http://localhost:{port}/");
                    httpListener.Start();
                    freePort = port;
                    Logger.LogTrace("Port [{port}] is free", port);
                    break;
                }
                catch
                {
                    Logger.LogWarning("Port [{port}] is busy", port);
                }
            }
            if (freePort == 0)
            {
                httpListener.Close();
                Logger.LogWarning("All ports are busy in ginve range [{ports}]", ports);
                result.ErrorDescription = $"All ports are busy in given range [{string.Join(',', ports)}]";
                return result;
            }
            Logger.LogTrace("Generating unique state for OAuth");
            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            Logger.LogTrace("Generated state: [{state}]", generatedState);
            Logger.LogTrace("Generating url");
            var sb = new StringBuilder()
                .Append($"{BaseAddress}auth?")
                .Append($"response_type=code&client_id={ClientId}&scope={Scope}&state={generatedState}&redirect_uri=http://localhost:{freePort}");
            OAuthLinkGenerated?.Invoke(this, sb.ToString());
            Logger.LogTrace("Generated url: [{url}]", sb.ToString());
            Logger.LogTrace("Starting process to open OAuth page");
            Process.Start(new ProcessStartInfo
            {
                FileName = sb.ToString(),
                UseShellExecute = true,
            });
            Logger.LogTrace("Waiting reponse from FAF OAuth");
            var task = Task.Run(async () => await httpListener.GetContextAsync());
            await task.WaitAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                httpListener.Close();
                Logger.LogTrace("Operation was cancelled");
                result.ErrorDescription = "Operation was canlled";
                return result;
            }
            var context = task.Result;
            Logger.LogTrace("Get response from [{url}]", context.Request.RawUrl);
            var response = context.Response;
            var request = context.Request;
            if (response.StatusCode == 200)
            {
                Logger.LogTrace("Getting code from query parameters", @request.QueryString);
                var code = request.QueryString["code"];
                if (code is null)
                {
                    Logger.LogWarning("Code not found");
                    result.ErrorDescription = "Code not found in OAuth response";
                    return result;
                }
                Logger.LogTrace("Code: [{code}]", code);
                Logger.LogTrace("Sending response page");
                System.Windows.Application.ResourceAssembly
                    .GetManifestResourceStream("Ethereal.FAF.UI.Client.Resources.OAuthResult.html")
                    .CopyTo(context.Response.OutputStream);
                Logger.LogTrace("Closing response");
                context.Response.Close();
                var token = await FetchOAuthToken(code, refreshing: false, freePort);
                result.IsError = false;
                result.TokenBearer = token;
                return result;
            }
            return result;
        }
        private async Task<TokenBearer> FetchOAuthToken(string code, bool refreshing = false, int port = 0, CancellationToken cancellationToken = default)
        {
            Logger.LogTrace("Fetching token using {schema} [{code}]", refreshing ? $"refresh token" : $"code", code);
            using var client = HttpClientFactory.CreateClient();
            Logger.LogTrace("Base address [{}]", BaseAddress);
            client.BaseAddress = new Uri(BaseAddress);
            string parameters =
                (refreshing ?
                    "grant_type=refresh_token&refresh_token=" :
                    $"grant_type=authorization_code&redirect_uri=http://localhost:{port}&code=")
                + code
                + $"&client_id={ClientId}";
            Logger.LogTrace("Generated content form [{}]", parameters);
            ByteArrayContent byteArrayContent = new(Encoding.UTF8.GetBytes(parameters));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync("token", byteArrayContent, cancellationToken);
            return await JsonSerializer.DeserializeAsync<TokenBearer>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        }
    }
}
