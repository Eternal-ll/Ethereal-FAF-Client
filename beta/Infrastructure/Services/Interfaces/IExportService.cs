using beta.Models.API;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Export service
    /// </summary>
    internal interface IExportService
    {
        /// <summary>
        /// Exports maps to local .docx file
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        public Task ExportMaps(ApiMapData[] maps);
    }
}
