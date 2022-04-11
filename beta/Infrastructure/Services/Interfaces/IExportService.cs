using beta.Models.API;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    internal interface IExportService
    {
        public Task ExportMaps(ApiMapData[] maps);
    }
}
