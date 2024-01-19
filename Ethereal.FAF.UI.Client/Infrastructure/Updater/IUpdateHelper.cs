using System;
using System.Threading.Tasks;
using Ethereal.FAF.UI.Client.Models.Progress;

namespace Ethereal.FAF.UI.Client.Infrastructure.Updater
{

    /// <summary>
    /// Client updater
    /// </summary>
    public interface IUpdateHelper
    {
        event EventHandler<UpdateStatusChangedEventArgs> UpdateStatusChanged;
        Task StartCheckingForUpdates();
        Task CheckForUpdate();
        Task DownloadUpdate(UpdateInfo updateInfo, IProgress<ProgressReport> progress);
    }
}
