using beta.Infrastructure.Services.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class ApiService : IApiService
    {
        private HttpClient GetHttpClient()
        {
            HttpClient client = new();

            client.DefaultRequestHeaders
                .Accept
                //ACCEPT header
                .Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

            client.DefaultRequestHeaders.Add("User-Agent", "FAF Client");
            var token = Properties.Settings.Default.access_token;
            if (token is not null)
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer token");
            }
            return client;
        }

        public async Task<string> GET(string path)
        {
            try
            {
                using HttpClient client = GetHttpClient();
                
                using HttpRequestMessage request = new(HttpMethod.Get, "https://api.faforever.com/" + path);

                var token = Properties.Settings.Default.access_token;
                if (token is not null)
                {
                    request.Headers.Authorization = new("Bearer", token);
                }

                //CONTENT-TYPE header
                //request.Content = new StringContent("{\"name\":\"John Doe\",\"age\":33}", Encoding.UTF8, "application/vnd.api+json");


                var response = await client.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                //using StreamReader sr = new(stream);
                //var json = sr.ReadToEnd();
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
                return null;
            }
        }
    }
}
