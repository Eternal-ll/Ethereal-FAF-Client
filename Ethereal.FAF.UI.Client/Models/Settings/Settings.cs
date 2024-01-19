using Ethereal.FAF.UI.Client.Models.Update;
using Newtonsoft.Json;
using Nucs.JsonSettings;
using System.Collections.Generic;
using System.ComponentModel;

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


        #region UI

        public virtual bool IsNavExpanded { get; set; }

        #endregion
    }
}
