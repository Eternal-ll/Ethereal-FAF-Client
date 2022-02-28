using beta.Infrastructure.Services.Interfaces;
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

        //public string GET(string path)
        //{

        //}
    }
}
