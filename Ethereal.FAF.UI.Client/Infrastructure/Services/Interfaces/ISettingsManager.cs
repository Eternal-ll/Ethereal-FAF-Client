using Ethereal.FAF.UI.Client.Models.Settings;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface ISettingsManager
    {
        public Settings Settings { get; }
        public RemoteClientConfiguration ClientConfiguration { get; }
        public Task LoadAsync();
    }
}
