using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

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
        public event EventHandler<EventArgs<OAuthState>> Result;

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

        #region SafeRequest methods
        private Stream SafeRequest(string requestUri) => SafeRequest(requestUri, null);
        private Stream SafeRequest(string requestUri, ByteArrayContent data = null)
        {
            try
            {
                if (data is not null)
                    return HttpClient.PostAsync(requestUri, data).Result.Content.ReadAsStream();
                return HttpClient.GetStreamAsync(requestUri).Result;
            }
            catch (Exception e)
            {
                if (e is HttpRequestException) OnResult(OAuthState.NO_CONNECTION);
                else if (e is AggregateException) OnResult(OAuthState.NO_CONNECTION);
                else OnResult(OAuthState.INVALID);
            }

            return null;
        }
        #endregion

        private string GetOAuthCode(string usernameOrEmail, string password)
        {
            Logger.LogInformation("Starting process of getting OAuth code");
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                Logger.LogWarning("Given usernameOrEmail or password is empty or null");
                Result?.Invoke(this, OAuthState.EMPTY_FIELDS);
                return null;
            }

            Logger.LogInformation("Generating unique uid to compare with result");

            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            Logger.LogInformation(nameof(generatedState) + " - " + generatedState);

            #region GET hydra.faforever.com/oauth2/auth

            Logger.LogInformation("GET https://hydra.faforever.com/oauth2/auth");

            var stream = SafeRequest("https://hydra.faforever.com/oauth2/auth?" +
                            "response_type=code&" +
                            "client_id=faf-python-client&" +
                            "redirect_uri=http://localhost&" +
                            "scope=openid+offline+public_profile+lobby&" +
                            "state=" + generatedState);

            if (stream is null) return null;

            var streamReader = new StreamReader(stream);

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

            Logger.LogInformation("_csrf - " + _csrf);
            Logger.LogInformation("login_challenge - " + login_challenge);

            if (_csrf.Length == 0 || login_challenge.Length == 0)
                return null;

            #endregion

            #region POST user.faforever.com/oauth2/login

            Logger.LogInformation("POST https://user.faforever.com/oauth2/login");
            StringBuilder builder = new();

            streamReader = new StreamReader(SafeRequest("https://user.faforever.com/oauth2/login", builder
                .Append("_csrf=").Append(_csrf)
                .Append("&login_challenge=").Append(login_challenge)
                .Append("&usernameOrEmail=").Append(usernameOrEmail)
                .Append("&password=").Append(password).GetQueryByteArrayContent()));

            string consent_challenge = string.Empty;

            Logger.LogInformation("Parsing data to get consent_challenge");

            while ((line = streamReader.ReadLine()) is not null)
                if (consent_challenge.Length != 0)
                    break;
                else if (line.Contains(nameof(consent_challenge))) 
                    consent_challenge = GetHiddenValue(line);

            Logger.LogInformation(nameof(consent_challenge) + " - " + consent_challenge);

            if (consent_challenge.Length == 0)
            {
                Result?.Invoke(this, OAuthState.INVALID);
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
                Logger.LogWarning("Something went wrong on POST request");
                if (e is HttpRequestException) OnResult(OAuthState.NO_CONNECTION);
                else OnResult(OAuthState.INVALID);

            }
            
            string code = callback?.Query.Substring(6, callback.Query.IndexOf("&scope", StringComparison.Ordinal) - 6);


            Logger.LogInformation("code - " + code);
            //int statePos = callback!.Query.IndexOf("state=", StringComparison.Ordinal);
            //string state = callback.Query.Substring(statePos, 
            //    callback.Query.Length - statePos);
            //state = state.Remove(0, 6);

            //Debug.WriteLine(nameof(code) + " - " + code);
            //Debug.WriteLine(nameof(state) + " - " + state);
            #endregion

            return code;
        }
        public void FetchOAuthToken(string code)
        {
            Logger.LogInformation("Starting fetching OAuth token by code");

            OAuthState result = OAuthState.INVALID;

            Logger.LogInformation("POST https://hydra.faforever.com/oauth2/token");

            if (FetchOAuthPayload(SafeRequest("https://hydra.faforever.com/oauth2/token",
                OAuthExtension.GetQueryByteArrayContent($"grant_type=authorization_code&code={code}&client_id=faf-python-client&redirect_uri=http://localhost"))))
                result = OAuthState.AUTHORIZED;
            else Logger.LogWarning("Something went wrong in the JSON data parsing part");
            
            Logger.LogInformation(result.ToString());
            
            Result?.Invoke(this, result);
        }
        public void RefreshOAuthToken(string refresh_token)
        {
            Logger.LogInformation("Starting refreshing OAuth token by refresh token");

            if (string.IsNullOrWhiteSpace(refresh_token))
            {
                throw new ArgumentNullException(nameof(refresh_token));
            }

            OAuthState result = OAuthState.INVALID;

            Logger.LogInformation("POST https://hydra.faforever.com/oauth2/token");

            if (FetchOAuthPayload(SafeRequest("https://hydra.faforever.com/oauth2/token",
                OAuthExtension.GetQueryByteArrayContent($"grant_type=refresh_token&refresh_token={refresh_token}&client_id=faf-python-client&redirect_uri=http://localhost"))))
                result = OAuthState.AUTHORIZED;
            else Logger.LogWarning("Something went wrong in the JSON data parsing part");

            Logger.LogInformation(result.ToString());

            Result?.Invoke(this, result);
        }
      
        public void Auth()
        {
            // TODO: move or we are fine? xD
            if (string.IsNullOrEmpty(Settings.Default.access_token))
                if (!string.IsNullOrEmpty(Settings.Default.refresh_token))
                    RefreshOAuthToken(Settings.Default.refresh_token);
                else Result!.Invoke(this, OAuthState.NO_TOKEN);
            else if ((Settings.Default.expires_in - DateTime.UtcNow).TotalSeconds < 10)
                // TODO FIX ME Not working
                RefreshOAuthToken(Settings.Default.refresh_token);
            else Result!.Invoke(this, OAuthState.AUTHORIZED);
        }

        public void Auth(string access_token)
        {
            // TODO: KAPPA
        }

        public void Auth(string usernameOrEmail, string password)
        {
            Logger.LogInformation("Starting process of authorization");

            var code = GetOAuthCode(usernameOrEmail, password);
            if (code is null)
            {
                // TODO: invoke some error events?
                return;
            }

            //Settings.Default.PlayerPassword = password;

            FetchOAuthToken(code);
        }

        protected virtual void OnResult(EventArgs<OAuthState> e) => Result?.Invoke(this, e);

        private string GetHiddenValue(string line)
        {
            // TODO: rewrite me
            int? pos = line!.IndexOf("value=\"", StringComparison.Ordinal) + 7;
            return pos.HasValue ? line.Substring(pos.Value, line.Length - 3 - pos.Value) : null;
        }

        private bool FetchOAuthPayload(Stream sr)
        {
            //  "access_token": "**********",
            //    "expires_in": "**********",
            //      "id_token": "**********",
            // "refresh_token": "***********",
            //         "scope": "openid offline public_profile lobby",
            //    "token_type": "bearer"

            StringBuilder sb = new();
            StringBuilder cb = new();
            char[] buffer = new char[1];
            byte[] byteBuffer = new byte[1];
            string keyword = string.Empty;

            string[] payload = new string[4];

            while (sr.CanRead)
            {
                sr.Read(byteBuffer, 0, 1);
                buffer[0] = Convert.ToChar(byteBuffer[0]);

                if (keyword.Length > 0)
                {
                    if (buffer[0] == '\"' && cb.Length > 3)
                    {
                        switch (keyword)
                        {
                            case "\"error": return false;
                            case "\"access_token":
                                payload[0] = cb.Remove(0, 2).ToString();
                                break;
                            case "\",\"expires_in":
                                payload[1] = cb.Remove(0, 1).ToString().Replace(",", null);
                                break;
                            case "\"id_token":
                                payload[2] = cb.Remove(0, 2).ToString();
                                break;
                            case "\",\"refresh_token":
                                payload[3] = cb.Remove(0, 2).ToString();
                                break;
                        }
                        cb.Clear();
                        keyword = string.Empty;
                        if (payload[3] is not null)
                        {
                            break;
                        }
                        continue;
                    }
                    cb.Append(buffer[0]);
                    continue;
                }

                if (sb.Length > 0)
                {
                    if (buffer[0] == '\"' && sb.Length > 2)
                    {
                        keyword = sb.ToString();
                        sb.Clear();
                    }
                    sb.Append(buffer[0]);
                    continue;
                }

                if (buffer[0] == '\"')
                {
                    sb.Append(buffer[0]);
                }
            }

            Settings.Default.access_token = payload[0];
            Settings.Default.expires_in = DateTime.UtcNow.AddSeconds(double.Parse(payload[1]));
            Settings.Default.id_token = payload[2];
            Settings.Default.refresh_token = payload[3];

            return true;
        }
    }
}
