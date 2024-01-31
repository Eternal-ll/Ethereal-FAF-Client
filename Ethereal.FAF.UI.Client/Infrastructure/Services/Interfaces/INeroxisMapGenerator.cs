using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface INeroxisMapGenerator
    {
        public bool IsNeroxisGeneratedMap(string map);
        /// <summary>
        /// Generate map
        /// </summary>
        /// <param name="map">Generated map name with seed</param>
        /// <param name="folder">Target folder for generated map. Default value taken from congifuration</param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task GenerateMapAsync(
            string name,
            string folder,
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default);
    }
}
