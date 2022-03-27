using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Models.OAuth;
using beta.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public static class OAuthExtension
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

        private async Task<Stream> SafeRequest(string requestUri) => await SafeRequest(requestUri, null);
        private async Task<Stream> SafeRequest(string requestUri, ByteArrayContent data = null)
        {
            Logger.LogInformation($"POST {requestUri}, data: \n{data?.ReadAsStringAsync().Result}");
            try
            {
                if (data is not null)
                    return await (await HttpClient.PostAsync(requestUri, data)).Content.ReadAsStreamAsync();
                return await HttpClient.GetStreamAsync(requestUri);
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message, e.StackTrace);
                if (e is HttpRequestException || e is AggregateException)
                {
                    OnStateChanged(new(OAuthState.NO_CONNECTION, "No connection", e.StackTrace));
                }
                else
                {
                    OnStateChanged(new(OAuthState.INVALID, $"Something went wrong on {requestUri} endpoint", e.StackTrace));
                }
            }

            return null;
        }

        private async Task<string> GetOAuthCodeAsync(string usernameOrEmail, string password)
        {
            Logger.LogInformation("Getting OAuth code by username or e-mail and password");
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                Logger.LogWarning("Given usernameOrEmail and password is empty");
                OnStateChanged(new(OAuthState.EMPTY_FIELDS, "Fill required fields"));
                return null;
            }

            Logger.LogInformation("Generating unique uid to compare with result");

            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            Logger.LogInformation(@generatedState);

            #region GET hydra.faforever.com/oauth2/auth

            var stream = await SafeRequest(
                @"https://hydra.faforever.com/oauth2/auth?response_type=code&client_id=faf-python-client&redirect_uri=http://localhost&scope=openid+offline+public_profile+lobby&state=" + generatedState);

            if (stream is null) return null;

            StreamReader streamReader = new(stream);

            string line;

            string _csrf = string.Empty;
            string login_challenge = string.Empty;

            Logger.LogInformation("Parsing data to get _csrf and login_challenge");

            while ((line = streamReader.ReadLine()) is not null)
                if (_csrf.Length != 0 && login_challenge.Length != 0)
                    break; 
                else if (line.Contains(nameof(_csrf)))
                    _csrf = GetHiddenValue(line);
                else if (line.Contains(nameof(login_challenge)))
                    login_challenge = GetHiddenValue(line);

            streamReader.Dispose();
            await stream.DisposeAsync();

            if (_csrf.Length == 0 || login_challenge.Length == 0)
            {
                Logger.LogWarning("CSRF or login_challenge not found in response");
                return null;
            }

            #endregion

            #region POST user.faforever.com/oauth2/login
            StringBuilder builder = new();

            stream = await SafeRequest("https://user.faforever.com/oauth2/login", builder
                .Append("_csrf=").Append(_csrf)
                .Append("&login_challenge=").Append(login_challenge)
                .Append("&usernameOrEmail=").Append(usernameOrEmail)
                .Append("&password=").Append(password).GetQueryByteArrayContent());

            if (stream is null) return null;

            streamReader = new StreamReader(stream);

            string consent_challenge = string.Empty;

            Logger.LogInformation("Parsing data to get consent_challenge");

            while ((line = streamReader.ReadLine()) is not null)
                if (consent_challenge.Length != 0)
                    break;
                else if (line.Contains(nameof(consent_challenge))) 
                    consent_challenge = GetHiddenValue(line);

            streamReader.Dispose();
            await stream.DisposeAsync();

            Logger.LogInformation(nameof(consent_challenge) + " - " + consent_challenge);

            if (consent_challenge.Length == 0)
            {
                OnStateChanged(new(OAuthState.INVALID, "Wrong user data"));
                return null;
            }
            #endregion

            #region POST user.faforever.com/oauth2/consent

            Logger.LogInformation("POST https://user.faforever.com/oauth2/consent");

            Uri callback = null;
            try
            {
                callback = HttpClient.PostAsync("https://user.faforever.com/oauth2/consent", builder.Clear()
                        .Append("_csrf=").Append(_csrf)
                        .Append("&action=permit&")
                        .Append("authorize=Authorize&")
                        .Append("consent_challenge=").Append(consent_challenge)
                        .GetQueryByteArrayContent()).Result.Headers.Location;
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Something went wrong on POST user.faforever.com/oauth2/consent");
                if (e is HttpRequestException || e is AggregateException)
                {
                    OnStateChanged(new(OAuthState.NO_CONNECTION, "No connection", e.StackTrace));
                }
                else
                {
                    OnStateChanged(new(OAuthState.INVALID, "Something went wrong", e.StackTrace));
                }
            }

            if (callback is null) return null;
            
            string code = callback.Query[6..callback.Query.IndexOf("&scope", StringComparison.Ordinal)];


            Logger.LogInformation("code - " + code);

            //Debug.WriteLine(nameof(code) + " - " + code);
            //Debug.WriteLine(nameof(state) + " - " + state);
            #endregion

            return code;
        }
        //private async Task<bool> FetchOAuthTokenAsync(string code)
        //{
        //    Logger.LogInformation("Fetching OAuth token by code");

        //    if (code is null) return false;

        //    return await FetchOAuthDataAsync(code);
        //}

        public async Task<bool> RefreshOAuthTokenAsync(string refresh_token)
        {
            Logger.LogInformation("Refreshing OAuth token");

            return await FetchOAuthDataAsync(refresh_token, true);
        }
        private async Task<bool> FetchOAuthDataAsync(string data, bool isRefreshToken = false)
        {
            string type = isRefreshToken ? "grant_type=refresh_token&refresh_token=" : "grant_type=authorization_code&code=";
            type += data;

            var stream = await SafeRequest("https://hydra.faforever.com/oauth2/token",
                OAuthExtension.GetQueryByteArrayContent($"{type}&client_id=faf-python-client&redirect_uri=http://localhost"));

            return await TryParseTokenBearerAsync(stream);
        }
      
        public async Task AuthAsync()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.access_token) || string.IsNullOrWhiteSpace(Settings.Default.refresh_token))
            {
                OnStateChanged(new(OAuthState.NO_TOKEN, "No token"));
                return;
            }

            if ((Settings.Default.expires_in - DateTime.UtcNow).TotalSeconds < 10)
            {
                if (await RefreshOAuthTokenAsync(Settings.Default.refresh_token))
                {

                }
                else
                {
                    OnStateChanged(new(OAuthState.INVALID, "Cant authorize using token"));
                }
            }
            else OnStateChanged(new(OAuthState.AUTHORIZED, "Authorized"));
        }

        public async Task AuthAsync(string usernameOrEmail, string password)
        {
            Logger.LogInformation("Starting process of authorization");

            var code = await GetOAuthCodeAsync(usernameOrEmail, password);
            if (code is null)
            {
                // TODO: invoke some error events?
                return;
            }
            await FetchOAuthDataAsync(code);
            //OnStateChanged(await FetchOAuthTokenAsync(code)
            //    ? (new(OAuthState.AUTHORIZED, "Authorized"))
            //    : (new(OAuthState.INVALID, "Something went wrong. Check internet access to https://hydra.faforever.com/oauth2/token")));
        }

        //public void DoAuth(string usernameOrEmail, string password)
        //{
        //    // Поток авторизации чем-то занят и будущем надо будет уведомить пользователя об этом
        //    if (AuthorizationThread is not null)
        //    {
        //        throw new NotImplementedException();
        //    }
        //    // создаем поток авторизации и передаем параметры в метод
        //    AuthorizationThread = new(()=> Auth(usernameOrEmail, password));
            
        //    // на самом деле до этого все было относительно костыльно,
        //    // я создавал поток сразу в AuthView для авторизации

        //    // здесь нужно поднимать какое-нибудь событие, что начинается процесс авторизации

        //}

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

        private async Task<bool> TryParseTokenBearerAsync(Stream stream)
        {
            if (stream is null) return false;

            try
            {
                var data = await JsonSerializer.DeserializeAsync<TokenBearer>(stream);
                await stream.DisposeAsync();

                if (data.AccessToken is null)
                {
                    throw new Exception();
                }

                Settings.Default.access_token = data.AccessToken;
                Settings.Default.expires_in = data.ExpiresAt;
                Settings.Default.id_token = data.IdToken;
                Settings.Default.refresh_token = data.RefreshToken;
                OnStateChanged(new(OAuthState.AUTHORIZED,""));
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                OnStateChanged(new(OAuthState.INVALID, ex.Message, ex.StackTrace));
                await stream.DisposeAsync();
            }
            return false;
        }
    }
}
