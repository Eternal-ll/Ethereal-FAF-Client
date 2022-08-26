using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Light.Infrastructure.OAuth
{
    public class OAuthResult
    {
        public TokenBearer TokenBearer { get; set; }
        public bool IsError { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
    public class FafOAuthClient
    {
        private readonly string ClientId;
        private readonly string Scope;
        private readonly int[] RedirectPorts;
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly IConfiguration Configuration;

        public TokenBearer TokenBearer;

        public FafOAuthClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            Configuration = configuration;
            HttpClientFactory = httpClientFactory;

            ClientId = configuration.GetValue<string>("OAuth:ClientID");
            Scope = configuration.GetValue<string>("OAuth:Scope");
            RedirectPorts = configuration.GetSection("OAuth:RedirectPorts").Get<int[]>();
        }

        public async Task<TokenBearer> AuthByBrowser(CancellationToken cancellationToken)
        {
            using var httpListener = new HttpListener();
            var ports = RedirectPorts;
            var freePort = 0;
            foreach (var port in ports)
            {
                try
                {
                    httpListener.Prefixes.Clear();
                    httpListener.Prefixes.Add($"http://localhost:{port}/");
                    httpListener.Start();
                    freePort = port;
                    break;
                }
                catch
                {

                }
            }
            if (freePort == 0)
            {
                throw new Exception("All required ports are closed");
            }
            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var sb = new StringBuilder()
                .Append("https://hydra.faforever.com/oauth2/auth?")
                .Append($"response_type=code&client_id={ClientId}&scope={Scope}&state={generatedState}&redirect_uri=http://localhost:{freePort}&");
            Process.Start(new ProcessStartInfo
            {
                FileName = sb.ToString(),
                UseShellExecute = true,
            });
            var task = Task.Run(async () => await httpListener.GetContextAsync());
            await task.WaitAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            var context = task.Result;

            var response = context.Response;
            var request = context.Request;
            if (response.StatusCode == 200)
            {
                //?code=3rHPSzZNaFNJLft6ESkP0Dg9yv-k676EHhlVMSWtRmA.zSPeJ-K0cg0Ed-MhtppRROLRFCTlgWrIBMQDiZbrQTo&scope=openid+offline+public_profile+lobby&state=9g5VGjFTy067aQilMTbcQA%3D%3D
                var code = request.QueryString["code"];
                if (code is null) return null;
                return await FetchOAuthToken(code, false, freePort);
            }
            return null;
        }
        private async Task<TokenBearer> FetchOAuthToken(string data, bool isRefreshToken = false, int port = 0)
        {
            using var client = HttpClientFactory.CreateClient();
            string type = isRefreshToken ? "grant_type=refresh_token&refresh_token=" : "grant_type=authorization_code&code=";
            type += data;
            ByteArrayContent byteArrayContent = new(Encoding.UTF8.GetBytes($"{type}&client_id={ClientId}&redirect_uri=http://localhost{(port == 0 ? "" : $":{port}")}"));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var response = await client.PostAsync("https://hydra.faforever.com/oauth2/token", byteArrayContent);
            return await JsonSerializer.DeserializeAsync<TokenBearer>(await response.Content.ReadAsStreamAsync());
        }
    }
}
