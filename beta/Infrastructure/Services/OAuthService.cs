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
    internal static class OAuthExtension
    {
        public static ByteArrayContent GetQueryByteArrayContent(string text)
        {
            ByteArrayContent byteArrayContent = new(Encoding.UTF8.GetBytes(text));
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return byteArrayContent;
        }
    }
    public class OAuthService : IOAuthService
    {
        public event EventHandler<OAuthEventArgs> StateChanged;

        private readonly HttpClient HttpClient = new(new HttpClientHandler { UseProxy = false });

        private readonly ILogger Logger;

        public OAuthService(ILogger<OAuthService> logger)
        {
            Logger = logger;
        }

        #region Encoding table
        // TODO: So, in some request we are getting html encoded strings (On code) like "=" as "%3D" and i`m thinking about encode them, but it is not necessary.

        //        Unencoded UrlEncoded UrlEncodedUnicode UrlPathEncoded EscapedDataString EscapedUriString HtmlEncoded HtmlAttributeEncoded HexEscaped
        //A         A          A                 A              A                 A                A           A                    %41
        //B         B          B                 B              B                 B                B           B                    %42
        //a         a          a                 a              a                 a                a           a                    %61
        //b         b          b                 b              b                 b                b           b                    %62
        //0         0          0                 0              0                 0                0           0                    %30
        //1         1          1                 1              1                 1                1           1                    %31
        //[space]   +          +                 %20            %20               %20              [space]     [space]              %20
        //!         !          !                 !              !                 !                !           !                    %21
        //"         %22        %22               "              %22               %22              &quot;      &quot;               %22
        //#         %23        %23               #              %23               #                #           #                    %23
        //$         %24        %24               $              %24               $                $           $                    %24
        //%         %25        %25               %              %25               %25              %           %                    %25
        //&         %26        %26               &              %26               &                &amp;       &amp;                %26
        //'         %27        %27               '              '                 '                &#39;       &#39;                %27
        //(         (          (                 (              (                 (                (           (                    %28
        //)         )          )                 )              )                 )                )           )                    %29
        //*         *          *                 *              %2A               *                *           *                    %2A
        //+         %2b        %2b               +              %2B               +                +           +                    %2B
        //,         %2c        %2c               ,              %2C               ,                ,           ,                    %2C
        //-         -          -                 -              -                 -                -           -                    %2D
        //.         .          .                 .              .                 .                .           .                    %2E
        // /         %2f        %2f               /              %2F               /                /           /                    %2F
        //:         %3a        %3a               :              %3A               :                :           :                    %3A
        //;         %3b        %3b               ;              %3B               ;                ;           ;                    %3B
        //<         %3c        %3c               <              %3C               %3C              &lt;        &lt;                 %3C
        //=         %3d        %3d               =              %3D               =                =           =                    %3D
        //>         %3e        %3e               >              %3E               %3E              &gt;        >                    %3E
        //?         %3f        %3f               ?              %3F               ?                ?           ?                    %3F
        //@         %40        %40               @              %40               @                @           @                    %40
        //[         %5b        %5b               [              %5B               %5B              [           [                    %5B
        //\         %5c        %5c               \              %5C               %5C              \           \                    %5C
        //]         %5d        %5d               ]              %5D               %5D              ]           ]                    %5D
        //^         %5e        %5e               ^              %5E               %5E              ^           ^                    %5E
        //_         _          _                 _              _                 _                _           _                    %5F
        //`         %60        %60               `              %60               %60              `           `                    %60
        //{         %7b        %7b               {              %7B               %7B              {           {                    %7B
        //|         %7c        %7c               |              %7C               %7C              |           |                    %7C
        //}         %7d        %7d               }              %7D               %7D              }           }                    %7D
        //~         %7e        %7e               ~              ~                 ~                ~           ~                    %7E
        //Ā         %c4%80     %u0100            %c4%80         %C4%80            %C4%80           Ā           Ā                    [OoR]
        //ā         %c4%81     %u0101            %c4%81         %C4%81            %C4%81           ā           ā                    [OoR]
        //Ē         %c4%92     %u0112            %c4%92         %C4%92            %C4%92           Ē           Ē                    [OoR]
        //ē         %c4%93     %u0113            %c4%93         %C4%93            %C4%93           ē           ē                    [OoR]
        //Ī         %c4%aa     %u012a            %c4%aa         %C4%AA            %C4%AA           Ī           Ī                    [OoR]
        //ī         %c4%ab     %u012b            %c4%ab         %C4%AB            %C4%AB           ī           ī                    [OoR]
        //Ō         %c5%8c     %u014c            %c5%8c         %C5%8C            %C5%8C           Ō           Ō                    [OoR]
        //ō         %c5%8d     %u014d            %c5%8d         %C5%8D            %C5%8D           ō           ō                    [OoR]
        //Ū         %c5%aa     %u016a            %c5%aa         %C5%AA            %C5%AA           Ū           Ū                    [OoR]
        //ū         %c5%ab     %u016b            %c5%ab         %C5%AB            %C5%AB           ū           ū                    [OoR] 

        #endregion

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
            //try
            //{
                if (data is not null)
                    return await (await HttpClient.PostAsync(requestUri, data)).Content.ReadAsStreamAsync();
                return await HttpClient.GetStreamAsync(requestUri);
            //}
            //catch (Exception e)
            //{
            //    Logger.LogError(e.Message, e.StackTrace);
            //    if (e is HttpRequestException || e is AggregateException)
            //    {
            //        OnStateChanged(new(OAuthState.NO_CONNECTION, "No connection", e.StackTrace));
            //    }
            //    else
            //    {
            //        OnStateChanged(new(OAuthState.INVALID, $"Something went wrong on {requestUri} endpoint", e.StackTrace));
            //    }
            //    throw e;
            //}

            //return null;
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

            var stream = await SafeRequest("https://hydra.faforever.com/oauth2/token",
                OAuthExtension.GetQueryByteArrayContent($"{type}&client_id=ethereal-faf-client" +
                $"&redirect_uri=http://localhost{(port == 0 ? "" : $":{port}")}"));
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

            var context = await httpListener.GetContextAsync().AsCancellable(token);
            httpListener.Close();

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
