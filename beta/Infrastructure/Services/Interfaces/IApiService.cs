using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IApiService
    {
        public Task<string> GET(string path);
    }
}
