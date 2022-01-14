using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using beta.Properties;

namespace beta.Infrastructure.Services
{
    public class OAuthService : IOAuthService
    {
        public event EventHandler<EventArgs<OAuthStates>> Result;
        protected virtual void OnResult(EventArgs<OAuthStates> e) => Result?.Invoke(this, e);

        private static readonly HttpClient client = new(new HttpClientHandler { UseProxy = false });
        private readonly Dictionary<string, string> Data = new();
        private readonly Stopwatch timer = new();
        private readonly StringBuilder builder = new();
        
        private bool Initialized = false;
        public async Task InitialLaunch()
        {
            if (Initialized) return;

            await SafeRequest("https://localhost", new byte[]{0 });
            Initialized=true;
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
        
        private async Task<Stream> SafeRequest(string requestUri) =>
            await SafeRequest(requestUri, bytes:null);
        private async Task<Stream> SafeRequest(string requestUri, FormUrlEncodedContent data = null) 
        {
            try
            {
                if (data != null)
                    return await (await client
                        .PostAsync(requestUri, data)
                        .ConfigureAwait(false)).Content.ReadAsStreamAsync();
                return await client.GetStreamAsync(requestUri).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is HttpRequestException) OnResult(OAuthStates.NO_CONNECTION);
                else OnResult(OAuthStates.INVALID);
            }

            return null;
        }
        private async Task<Stream> SafeRequest(string requestUri, ByteArrayContent data = null) 
        {
            try
            {
                if (data != null)
                    return await (await client
                        .PostAsync(requestUri, data)
                        .ConfigureAwait(false)).Content.ReadAsStreamAsync();
                return await client.GetStreamAsync(requestUri).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is HttpRequestException) OnResult(OAuthStates.NO_CONNECTION);
                else OnResult(OAuthStates.INVALID);
            }

            return null;
        }

        private async Task<Stream> SafeRequest(string requestUri, byte[] bytes = null)
        {
            try
            {
                if (bytes != null)
                    return await (await client
                        .PostAsync(requestUri,new ByteArrayContent(bytes))
                        .ConfigureAwait(false)).Content.ReadAsStreamAsync();
                return await client.GetStreamAsync(requestUri).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is HttpRequestException) OnResult(OAuthStates.NO_CONNECTION);
                else OnResult(OAuthStates.INVALID);
            }

            return null;
        }

        //private ByteArrayContent GetQueryByteArrayContent(string query)
        //{
        //    ByteArrayContent bytes = new(Encoding.UTF8.GetBytes(query));
        //    bytes.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        //    return bytes;
        //}
        public async Task<string> GetOAuthCode(string usernameOrEmail, string password)
        {
            //timer.Restart();

            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                Result?.Invoke(this, OAuthStates.EMPTY_FIELDS);
                return null;
            }
            
            string generatedState = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            //Debug.WriteLine(nameof(generatedState) + " - " + generatedState);

            #region GET https://hydra.faforever.com/oauth2/auth

            //Debug.WriteLine("GET https://hydra.faforever.com/oauth2/auth");
            
            //Debug.WriteLine(timer.Elapsed.ToString("c"));
            var stream = await SafeRequest(builder.Clear()
            .Append("https://hydra.faforever.com/oauth2/auth?response_type=code&client_id=faf-python-client&redirect_uri=http%3A%2F%2Flocalhost&scope=openid+offline+public_profile+lobby&state=")
            .Append(generatedState).ToString());
            //Debug.WriteLine(timer.Elapsed.ToString("c"));
            if (stream is null) return null;

            StreamReader streamReader = new (stream);

            string line;

            string _csrf = string.Empty;
            string login_challenge = string.Empty;

            while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) != null)
                if (_csrf.Length != 0 && login_challenge.Length != 0)
                    break; 
                else if (line.Contains(nameof(_csrf)))
                    _csrf = GetHiddenValue(line);
                else if (line.Contains(nameof(login_challenge)))
                    login_challenge = GetHiddenValue(line);

            //Debug.WriteLine(nameof(_csrf) + " - " + _csrf);
            //Debug.WriteLine(nameof(login_challenge) + " - " + login_challenge);

            if (_csrf.Length == 0 && login_challenge.Length == 0)
                return null;

            #endregion
            
            //Debug.WriteLine(timer.Elapsed.ToString("c"));

            #region POST https://user.faforever.com/oauth2/login

            //Debug.WriteLine("POST https://user.faforever.com/oauth2/login");
            
            stream = await SafeRequest("https://user.faforever.com/oauth2/login", builder.Clear()
                .Append(nameof(_csrf)).Append("=").Append(_csrf).Append("&")
                .Append(nameof(login_challenge)).Append("=").Append(login_challenge).Append("&")
                .Append(nameof(usernameOrEmail)).Append("=").Append(usernameOrEmail).Append("&")
                .Append(nameof(password)).Append("=").Append(password).GetQueryByteArrayContent());

            if (stream is null) return null;

            streamReader = new StreamReader(stream);

            string consent_challenge = string.Empty;

            while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) != null)
                if (consent_challenge.Length != 0)
                    break;
                else if (line.Contains(nameof(consent_challenge))) 
                    consent_challenge = GetHiddenValue(line);

            //Debug.WriteLine(nameof(consent_challenge) + " - " + consent_challenge);

            if (consent_challenge.Length == 0)
            {
                Result?.Invoke(this, OAuthStates.INVALID);
                return null;
            }
            #endregion

            //Debug.WriteLine(timer.Elapsed.ToString("c"));

            #region POST https://user.faforever.com/oauth2/consent

            //Debug.WriteLine("POST https://user.faforever.com/oauth2/consent");
                        
            Uri callback = null;
            try
            {
                callback = (await client.PostAsync("https://user.faforever.com/oauth2/consent", builder.Clear()
                        .Append(nameof(_csrf)).Append("=").Append(_csrf).Append("&")
                        .Append("action=permit&")
                        .Append("authorize=Authorize&")
                        .Append(nameof(consent_challenge)).Append("=").Append(consent_challenge)
                        .GetQueryByteArrayContent())
                .ConfigureAwait(false)).Headers.Location;
            }
            catch (Exception e)
            {
                if (e is HttpRequestException) OnResult(OAuthStates.NO_CONNECTION);
                else OnResult(OAuthStates.INVALID);

            }
            
            string code = callback?.Query.Substring(6, callback.Query.IndexOf("&scope", StringComparison.Ordinal) - 6);

            //int statePos = callback!.Query.IndexOf("state=", StringComparison.Ordinal);
            //string state = callback.Query.Substring(statePos, 
            //    callback.Query.Length - statePos);
            //state = state.Remove(0, 6);
            
            //Debug.WriteLine(nameof(code) + " - " + code);
            //Debug.WriteLine(nameof(state) + " - " + state);
            #endregion

            //Debug.WriteLine(timer.Elapsed.ToString("c"));

            //timer.Stop();

            return code;
        }

        public async Task<string> FetchOAuthToken(string code)
        {
            //timer.Start();
            
            //Debug.WriteLine("POST https://hydra.faforever.com/oauth2/token");
            
            StreamReader streamReader = new(await SafeRequest("https://hydra.faforever.com/oauth2/token", builder.Clear()
                .Append("grant_type=authorization_code&code=").Append(code)
                    .Append("&client_id=faf-python-client&redirect_uri=http://localhost")
                    .GetQueryByteArrayContent()));
            
            var payload = await streamReader.ReadToEndAsync();

            // TODO: Maybe there is better and faster way to do this
            payload = payload.Substring(1, payload.Length - 2).Replace("\"", string.Empty);
            var strokes = payload.Split(",");

            //  access_token: **********
            //    expires_in: **********
            //      id_token: **********
            // refresh_token: ***********
            //         scope: openid offline public_profile lobby
            //    token_type: bearer

            //Debug.WriteLine(timer.Elapsed.ToString("c"));
            // TODO: and this
            Settings.Default.expires_in = DateTime.Now.AddSeconds(double.Parse(strokes[1].Split(":")[1]));
            Settings.Default.access_token = strokes[0].Split(":")[1];
            Settings.Default.id_token = strokes[2].Split(":")[1];
            Settings.Default.refresh_token = strokes[3].Split(":")[1];
            
            //timer.Reset();

            Result?.Invoke(this, OAuthStates.AUTHORIZED);

            return strokes[0].Split(":")[1];
        }

        public async Task<string> RefreshOAuthToken(string refresh_token)
        {
            //timer.Restart();
            //Debug.WriteLine("RefreshOAuthToken(string refresh_token)");
            if (string.IsNullOrWhiteSpace(refresh_token))
            {
                return null;
            }
            
            //Debug.WriteLine("POST https://hydra.faforever.com/oauth2/token");
            
            var stream = new StreamReader(await SafeRequest("https://hydra.faforever.com/oauth2/token", builder.Clear()
                .Append("grant_type=refresh_token&refresh_token=").Append(refresh_token).Append("&client_id=faf-python-client")
                .GetQueryByteArrayContent()));

            var payload = await stream.ReadToEndAsync();

            payload = payload.Substring(1, payload.Length - 2).Replace("\"", string.Empty);
            var strokes = payload.Split(",");
            //  access_token: **********
            //    expires_in: **********
            //      id_token: **********
            // refresh_token: ***********
            //         scope: openid offline public_profile lobby
            //    token_type: bearer
            if (strokes.Length != 6)
            {
                // TODO: invoke some error events?
                return null;
            }

            //Debug.WriteLine(timer.Elapsed.ToString("c"));

            // TODO: how about raise event and update settings in other service?
            Settings.Default.expires_in = DateTime.Now.AddSeconds(double.Parse(strokes[1].Split(":")[1]));
            Settings.Default.access_token = strokes[0].Split(":")[1];
            Settings.Default.id_token = strokes[2].Split(":")[1];
            Settings.Default.refresh_token = strokes[3].Split(":")[1];
            
            //timer.Reset();
            
            Result?.Invoke(this, OAuthStates.AUTHORIZED);

            return strokes[0].Split(":")[1];
        }
        
        public async Task Auth()
        {
            // TODO: move or we are fine? xD
            if (Settings.Default.AutoJoin)
            {
                if (string.IsNullOrEmpty(Settings.Default.access_token))
                    Result!.Invoke(this, OAuthStates.NO_TOKEN);
                else if ((Settings.Default.expires_in - DateTime.Now).TotalSeconds < 10)
                    await RefreshOAuthToken(Settings.Default.refresh_token).ConfigureAwait(false);
                else Result!.Invoke(this, OAuthStates.AUTHORIZED);
            }
        }

        public async Task Auth(string access_token)
        {
            // TODO: KAPPA
        }

        public async Task Auth(string usernameOrEmail, string password)
        {
            var code = await GetOAuthCode(usernameOrEmail, password);
            if (code == null)
            {
                // TODO: invoke some error events?
                return;
            }

            await FetchOAuthToken(code);
        }

        //public async Task<string> GenerateUID(string session)
        //{
        //    if (string.IsNullOrWhiteSpace(session)) return null;

        //    string result = null;

        //    // TODO: invoke some error events?
        //    if (!File.Exists(Environment.CurrentDirectory + "\\faf-uid.exe"))
        //        return null;

        //    Process process = new ();
        //    process.StartInfo.FileName = "faf-uid.exe";
        //    process.StartInfo.Arguments = session;
        //    process.StartInfo.UseShellExecute = false;
        //    process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.CreateNoWindow = true;
        //    process.Start();
        //    while (!process.StandardOutput.EndOfStream)
        //    {
                
        //        // TODO: i didnt get it, why it doesnt work on async. Looks like main dispatcher being busy and stucks
        //        result += process.StandardOutput.ReadLine();
        //    }
        //    process.Close();

        //    return result;
        //}

        //some tricky function
        private string GetHiddenValue(string line)
        {
            // TODO: rewrite me
            int? pos = line!.IndexOf("value=\"", StringComparison.Ordinal) + 7;
            return pos.HasValue ? line.Substring(pos.Value, line.Length - 3 - pos.Value) : null;
        }

    }
}
