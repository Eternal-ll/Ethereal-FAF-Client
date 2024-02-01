using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.Models.Update;
using Newtonsoft.Json;
using Nucs.JsonSettings;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Ethereal.FAF.UI.Client.Models.Settings
{
    public class Settings : JsonSettings, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonIgnore]
        public override string FileName { get; set; }

        public virtual UpdateChannel PreferredUpdateChannel { get; set; } = UpdateChannel.Stable;



        public virtual bool RememberSelectedFaServer { get; set; } = true;
        public virtual string SelectedFaServer { get; set; }
        public virtual List<FafAuthToken> AuthTokens { get; set; } = new();
        [JsonIgnore]
        public virtual List<Server> Servers { get; set; } = new();


        #region UI

        public virtual bool IsNavExpanded { get; set; }

        #endregion


        #region ForgedAlliance
        public virtual string FAForeverLocation { get; set; } = "D:\\FAForever";
        public virtual string ForgedAllianceLocation { get; set; } = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Supreme Commander Forged Alliance";

        public virtual string ForgedAllianceVaultLocation { get; set; } = "C:\\Users\\Eternal\\Documents\\My Games\\Gas Powered Games\\Supreme Commander Forged Alliance";
        [JsonIgnore]
        public virtual string ForgedAllianceMapsLocation => Path.Combine(ForgedAllianceVaultLocation, "maps");

        #endregion
    }
}
