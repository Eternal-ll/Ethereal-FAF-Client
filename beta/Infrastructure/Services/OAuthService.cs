using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Models.OAuth;
using beta.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class OAuthService : IOAuthService
    {
        public event EventHandler<OAuthEventArgs> StateChanged;

        private readonly HttpClient HttpClient = new(new HttpClientHandler { UseProxy = false });

        private readonly ILogger Logger;

        public OAuthService(ILogger<OAuthService> logger)
        {
            Logger = logger;
        }

        private TokenBearer TokenBearer;
        public void SetToken(string accessToken, string refreshToken, string idToken, double expiresIn) => TokenBearer = new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IdToken = idToken,
            ExpiresIn = expiresIn,
        };

        private async Task<Stream> SafeRequest(string requestUri, IProgress<string> progress = null) => await SafeRequest(requestUri, null, progress);
        private async Task<Stream> SafeRequest(string requestUri, ByteArrayContent data = null, IProgress<string> progress = null)
        {
            Logger.LogInformation($"POST {requestUri}, data: \n{data?.ReadAsStringAsync().Result}");
            if (data is not null)
                return await (await HttpClient.PostAsync(requestUri, data)).Content.ReadAsStreamAsync();
            return await HttpClient.GetStreamAsync(requestUri);
        }

        private async Task<string> GetOAuthCodeAsync(string usernameOrEmail, string password, IProgress<string> progress = null)
        {
            progress?.Report("Checking passed authorization data");
            Logger.LogInformation("Getting OAuth code by username or e-mail and password");
            
            if (string.IsNullOrWhiteSpace(usernameOrEmail) && string.IsNullOrWhiteSpace(password))
            {
                Logger.LogWarning("Given usernameOrEmail and password is empty");
                throw new ArgumentNullException(nameof(usernameOrEmail), "Given username or e-mail and password is empty");
            }
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                throw new ArgumentNullException(nameof(usernameOrEmail), "Given username or e-mail is empty");
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password), "Given password is empty");
            }

            Logger.LogInformation("Generating unique uid for user");
            progress?.Report("Generating unique uid for user");
            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            progress?.Report($"Unique uid: {generatedState[..4]}XXXX{generatedState[..^4]}");

            Logger.LogInformation(@generatedState);

            #region GET hydra.faforever.com/oauth2/auth
            progress?.Report("GET hydra faforever com/oauth2/auth");
            var url = @"https://hydra.faforever.com/oauth2/auth?response_type=code&client_id=ethereal-faf-client&redirect_uri=http://localhost&scope=openid+offline+public_profile+lobby+upload_map+upload_mod&state=" + generatedState;
            var stream = await SafeRequest(url, progress);

            _ = stream ?? throw new ArgumentNullException(nameof(stream), $"Stream is null on {url}");

            progress?.Report("Data received");
            StreamReader streamReader = new(stream);
            //var data = streamReader.ReadToEnd();
            string line;

            string _csrf = null;
            string challenge = null;

            Logger.LogInformation("Parsing data to get _csrf and challenge");

            progress?.Report("Parsing result to get \"_csrf\" and \"challenge\"");
            while ((line = streamReader.ReadLine()) is not null)
                if (_csrf is not null && challenge is not null)
                    break; 
                else if (line.Contains("_csrf"))
                {
                    _csrf = GetHiddenValue(line);
                    progress?.Report("\"_csrf\" is found, looking for \"challenge\"");
                }
                else if (line.Contains("challenge"))
                {   
                    challenge = GetHiddenValue(line);
                    progress?.Report("\"challenge\" is found");
                }

            streamReader.Dispose();
            await stream.DisposeAsync();

            if (_csrf is null && challenge is null)
            {
                throw new ArgumentNullException($"{nameof(_csrf)} and {nameof(challenge)}", $"\"_crsf\" and \"challenge\" not found on result of {url}");
            }
            else
            {
                _ = _csrf ?? throw new ArgumentNullException(nameof(_csrf), $"\"_crsf\" not found on result of {url}");
                _ = challenge ?? throw new ArgumentNullException(nameof(challenge), $"\"challenge\" not found on result of {url}");
            }

            #endregion

            #region POST user.faforever.com/oauth2/login
            StringBuilder builder = new();

            progress?.Report("Processing authorization on user faforever com/oauth2/login");
            stream = await SafeRequest("https://user.faforever.com/oauth2/login", builder
                .Append("_csrf=").Append(_csrf)
                .Append("&challenge=").Append(challenge)
                .Append("&usernameOrEmail=").Append(usernameOrEmail)
                .Append("&password=").Append(password)
                .GetQueryByteArrayContent());

            if (stream is null) return null;

            progress?.Report("Data received");
            streamReader = new StreamReader(stream);

            string consent_challenge = null;

            Logger.LogInformation("Parsing data to get consent_challenge");

            progress?.Report("Parsing data to get \"challenge\"");
            while ((line = streamReader.ReadLine()) is not null)
                if (consent_challenge is not null)
                    break;
                else if (line.Contains("challenge"))
                {
                    progress?.Report("\"challenge\" is found");
                    consent_challenge = GetHiddenValue(line);
                }

            streamReader.Dispose();
            await stream.DisposeAsync();

            Logger.LogInformation(nameof(consent_challenge) + " - " + consent_challenge);

            _ = consent_challenge ?? throw new ArgumentNullException(nameof(challenge), "\"challenge\" not found on user.faforever.com/oauth2/login. Your authorization data maybe uncorrect");
            if (consent_challenge is null)
            {
                //OnStateChanged(new(OAuthState.INVALID, "Something went wrong", "Field \"consent_challenge\" was empty"));
                return null;
            }
            #endregion

            #region POST user.faforever.com/oauth2/consent

            Logger.LogInformation("POST https://user.faforever.com/oauth2/consent");

            Uri callback = null;
            try
            {
                progress?.Report("Pending request for OAuth code");
                callback = HttpClient.PostAsync("https://user.faforever.com/oauth2/consent", builder.Clear()
                        .Append("_csrf=").Append(_csrf)
                        .Append("&action=permit&")
                        .Append("authorize=Authorize&")
                        .Append("challenge=").Append(consent_challenge)
                        .GetQueryByteArrayContent()).Result.Headers.Location;
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Something went wrong on POST user.faforever.com/oauth2/consent");
                if (e is HttpRequestException || e is AggregateException)
                {
                    //OnStateChanged(new(OAuthState.NO_CONNECTION, "No connection", e.StackTrace));
                    throw new Exception("Error on https://user.faforever.com/oauth2/consent. Check your internet");
                }
                else
                {
                    throw new Exception("Error on https://user.faforever.com/oauth2/consent.");
                    //OnStateChanged(new(OAuthState.INVALID, "Something went wrong", e.StackTrace));
                }
            }

            _ = callback ?? throw new ArgumentNullException(nameof(callback), "Check your \"login\" or \"e-mail\" and \"password\"");
            if (callback is null) return null;

            progress?.Report("Parsing callback for OAuth code");
            string code = callback.Query[6..callback.Query.IndexOf("&scope", StringComparison.Ordinal)];


            Logger.LogInformation("code - " + code);
            progress?.Report("OAuth code found");
            #endregion

            return code;
        }

        public async Task<TokenBearer> RefreshOAuthTokenAsync(string refresh_token)
        {
            Logger.LogInformation("Refreshing OAuth token");

            return await FetchOAuthDataAsync(refresh_token, true);
        }
        private async Task<TokenBearer> FetchOAuthDataAsync(string data, bool isRefreshToken = false, IProgress<string> progress = null, int port = 0)
        {
            if (data is null) return null;

            if (isRefreshToken)
            {
                progress?.Report("Fetching OAuth access token by refresh token");
            }
            else
            {
                progress?.Report("Fetching OAuth access token by user data");
            }
            string type = isRefreshToken ? "grant_type=refresh_token&refresh_token=" : "grant_type=authorization_code&code=";
            type += data;


            ByteArrayContent byteArrayContent = new(Encoding.UTF8.GetBytes($"{type}&client_id=ethereal-faf-client" +
                $"&redirect_uri=http://localhost{(port == 0 ? "" : $":{port}")}"));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var stream = await SafeRequest("https://hydra.faforever.com/oauth2/token",byteArrayContent);
            return await JsonSerializer.DeserializeAsync<TokenBearer>(stream);
        }
      
        public async Task<TokenBearer> AuthAsync(IProgress<string> progress)
        {
            OnStateChanged(new(OAuthState.PendingAuthorization, "Pending authorization using saved token"));
            if (TokenBearer is null)
            {
                return null;
            }

            if ((TokenBearer.ExpiresAt - DateTime.Now).TotalSeconds < 10)
            {
                return await RefreshOAuthTokenAsync(Settings.Default.refresh_token);
            }

            return TokenBearer;
        }

        public async Task<TokenBearer> AuthAsync(string usernameOrEmail, string password, CancellationToken? token, IProgress<string> progress = null)
        {
            OnStateChanged(new(OAuthState.PendingAuthorization, "Pending authorization using user credentials"));
            progress?.Report("Pending OAuth autorization by parsing");
            var code = await GetOAuthCodeAsync(usernameOrEmail, password, progress);
            var tokenBearer = await FetchOAuthDataAsync(code, false, progress);
            return tokenBearer;
        }

        private void OnStateChanged(OAuthEventArgs e)
        {
            Logger.LogInformation(e.State.ToString());
            StateChanged?.Invoke(this, e);
        }

        private string GetHiddenValue(string line)
        {
            int pos = line.IndexOf("value=\"", StringComparison.Ordinal) + 7;
            return pos == -1 ? null : line[pos..^3];
        }

        public async Task<TokenBearer> AuthByBrowser(CancellationToken token, IProgress<string> progress = null)
        {
            OnStateChanged(new(OAuthState.PendingAuthorization, "Pending authorization using browser callbacks"));

            progress?.Report("Checking available ports");
            int? avaiablePort = null;
            var ports = new int[] { 57728, 59573, 58256, 53037, 51360 };
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var listeners = ipGlobalProperties.GetActiveTcpListeners();
            for (int i = 0; i < ports.Length; i++)
            {
                var isFree = true;
                for (int j = 0; j < listeners.Length; j++)
                {
                    if (listeners[j].Port == ports[i])
                    {
                        isFree = false;
                        break;
                    }
                }
                if (!isFree) continue;
                avaiablePort = ports[i];
                break;
            }
            if (!avaiablePort.HasValue)
            {
                throw new ArgumentOutOfRangeException($"None of these ports are avaiable for listening:\n{string.Join(',', ports)}");
            }

            HttpListener httpListener = new();
            httpListener.Prefixes.Add($"http://localhost:{avaiablePort.Value}/");
            httpListener.Start();
            progress?.Report("HTTP listener started");

            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var url = "https://hydra.faforever.com/oauth2/auth?response_type=code&client_id=ethereal-faf-client&" +
                $"redirect_uri=http://localhost:{avaiablePort.Value}&scope=openid+offline+public_profile+lobby+upload_map+upload_mod&" +
                "state=" + generatedState;

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });

            progress?.Report("Waiting for callback from FAForever");

            HttpListenerContext context = null;
            bool isCanceled = false;
            await httpListener.GetContextAsync().AsCancellable(token)
                .ContinueWith(task =>
                {
                    isCanceled = task.IsCanceled;
                    if (!task.IsCanceled && !task.IsFaulted)
                    {
                        context = task.Result;
                        context.Response.StatusCode = 200;
                        App.ResourceAssembly
                            .GetManifestResourceStream("beta.Resources.OAuthResult.html")
                            .CopyTo(context.Response.OutputStream);
                        context.Response.Close();
                    }
                    httpListener.Close();
                });
            if (isCanceled)
            {
                return null;
            }
            if (context is null)
            {
                throw new Exception("Didnt get http listener context");
            }

            var response = context.Response;
            var request = context.Request;

            progress?.Report("Fetching OAuth token");
            if (response.StatusCode == 200)
            {
                //?code=3rHPSzZNaFNJLft6ESkP0Dg9yv-k676EHhlVMSWtRmA.zSPeJ-K0cg0Ed-MhtppRROLRFCTlgWrIBMQDiZbrQTo&scope=openid+offline+public_profile+lobby&state=9g5VGjFTy067aQilMTbcQA%3D%3D
                var query = request.Url.Query[1..];
                var data = query.Split('&');
                for (int i = 0; i < data.Length; i++)
                {
                    var paramData = data[i].Split('=');
                    if (paramData[0] == "code")
                    {
                        return await FetchOAuthDataAsync(paramData[1], false, progress, avaiablePort.Value);
                    }
                }
                return null;
            }
            else
            {
                OnStateChanged(new(OAuthState.INVALID, $"Status code: {response.StatusCode} on redirect from OAuth"));
            }
            return null;
        }
    }
}
