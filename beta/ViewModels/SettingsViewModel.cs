using beta.Properties;

namespace beta.ViewModels
{
    internal class SettingsViewModel : Base.ViewModel
    {
        #region IsAlwaysDownloadMapEnabled
        private bool _IsAlwaysDownloadMapEnable = Settings.Default.AlwaysDownloadMap;
        public bool IsAlwaysDownloadMapEnabled
        {
            get => _IsAlwaysDownloadMapEnable;
            set
            {
                if (Set(ref _IsAlwaysDownloadMapEnable, value))
                {
                    Settings.Default.AlwaysDownloadMap = value;
                }
            }
        }
        #endregion

        #region IsAlwaysDownloadModEnabled
        private bool _IsAlwaysDownloadModEnabled = Settings.Default.AlwaysDownloadMod;
        public bool IsAlwaysDownloadModEnabled
        {
            get => _IsAlwaysDownloadModEnabled;
            set
            {
                if (Set(ref _IsAlwaysDownloadModEnabled, value))
                {
                    Settings.Default.AlwaysDownloadMod = value;
                }
            }
        }
        #endregion

        #region IsAlwaysDownloadPatchEnabled
        private bool _IsAlwaysDownloadPatchEnabled  = Settings.Default.AlwaysDownloadPatch;
        public bool IsAlwaysDownloadPatchEnabled
        {
            get => _IsAlwaysDownloadPatchEnabled;
            set
            {
                if (Set(ref _IsAlwaysDownloadPatchEnabled, value))
                {
                    Settings.Default.AlwaysDownloadPatch = value;
                }
            }
        }
        #endregion

        #region PathToMaps
        private string _PathToMaps = Settings.Default.PathToMaps;
        public string PathToMaps
        {
            get => _PathToMaps;
            set
            {
                if (Set(ref _PathToMaps, value))
                {
                    Settings.Default.PathToMaps = value;
                }
            }
        }
        #endregion

        #region PathToMods
        private string _PathToMods = Settings.Default.PathToMods;
        public string PathToMods
        {
            get => _PathToMods;
            set
            {
                if (Set(ref _PathToMods, value))
                {
                    Settings.Default.PathToMods = value;
                }
            }
        }
        #endregion

        #region PathToPatch
        private string _PathToPatch = App.GetPathToFolder(Models.Folder.ProgramData);//Settings.Default.PathToMods;
        public string PathToPatch
        {
            get => _PathToPatch;
            set
            {
                if (Set(ref _PathToPatch, value))
                {
                    //Settings.Default. = value;
                }
            }
        }
        #endregion
    }
}
