using beta.Infrastructure.Services.Interfaces;
using System;
using System.IO;
using System.Net.Http;

namespace beta.Infrastructure.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient HttpClient;

        public ApiService()
        {
            HttpClient = new();

            //HttpClient.DefaultRequestHeaders.Add("User-Agent", "FAF Client");
            //HttpClient.DefaultRequestHeaders.Add("Content-Type", "application/vnd.api+json");
        }

        public string GET(string path)
        {
            try
            {
                using Stream stream = HttpClient.GetAsync("https://api.faforever.com/" + path).Result.Content.ReadAsStream();
                using StreamReader sr = new(stream);
                var json = sr.ReadToEnd();
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
