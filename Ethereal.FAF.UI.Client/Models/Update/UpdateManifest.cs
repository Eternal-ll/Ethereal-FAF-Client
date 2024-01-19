using Ethereal.FAF.UI.Client.Infrastructure.Updater;
using System.Collections.Generic;

namespace Ethereal.FAF.UI.Client.Models.Update
{
    /// <summary>
    /// Updates
    /// </summary>
    public class UpdateManifest
    {
        public Dictionary<UpdateChannel, UpdateInfo> Updates { get; set; }
    }
}
