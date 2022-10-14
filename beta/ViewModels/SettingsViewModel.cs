using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace beta.ViewModels
{
    public class SettingsViewModel : Base.ViewModel
    {
        public struct Pa__one
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public Brush Brush { get; set; }
            public string BrushKey { get; set; }
        }

        private readonly INotificationService NotificationService;
        private readonly MainViewModel MainViewModel;

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            NotificationService = App.Services.GetService<INotificationService>();
            MainViewModel = mainViewModel;

            FillTheme();
        }

        private void FillTheme()
        {
            var theme = new List<Pa__one> { };

            foreach (var singleBrushKey in _themeResources)
            {
                var singleBrush = Application.Current.Resources[singleBrushKey] as Brush;

                if (singleBrush == null)
                    continue;

                string description;

                if (singleBrush is SolidColorBrush solidColorBrush)
                    description =
                        $"R: {solidColorBrush.Color.R}, G: {solidColorBrush.Color.G}, B: {solidColorBrush.Color.B}";
                else
                    description = "Gradient";

                theme.Add(new Pa__one
                {
                    Title = "THEME",
                    Subtitle = description + "\n" + singleBrushKey,
                    Brush = singleBrush,
                    BrushKey = singleBrushKey
                });
            }

            ThemeBrushes = theme;
        }


        private IEnumerable<Pa__one> _themeBrushes = new Pa__one[] { };
        public IEnumerable<Pa__one> ThemeBrushes
        {
            get => _themeBrushes;
            set => Set(ref _themeBrushes, value);
        }

        #region Authorization
        private bool _IsAutoJoin = Settings.Default.AutoJoin;
        public bool IsAutoJoin
        {
            get => _IsAutoJoin;
            set
            {
                if (Set(ref _IsAutoJoin, value))
                {
                    Settings.Default.AutoJoin = value;
                }
            }
        }
        #endregion

        #region Downloads

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
        private string _PathToPatch = App.GetPathToFolder(Folder.ProgramData);//Settings.Default.PathToMods;
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

        #region PathToGame
        private string _PathToGame = Settings.Default.PathToGame;
        public string PathToGame
        {
            get => _PathToGame;
            set
            {
                if (Set(ref _PathToGame, value))
                {
                    Settings.Default.PathToGame = value;
                }
            }
        }
        #endregion 
        #endregion

        #region IRC chat

        #region IsAlwaysConnectToIRC
        private bool _IsAlwaysConnectToIRC = Settings.Default.ConnectIRC;
        public bool IsAlwaysConnectToIRC
        {
            get => _IsAlwaysConnectToIRC;
            set
            {
                if (Set(ref _IsAlwaysConnectToIRC, value))
                {
                    Settings.Default.ConnectIRC = value;
                }
            }
        }
        #endregion

        #endregion

        #region Commands

        #region SelectPathToGameCommand
        private ICommand _SelectPathToGameCommand;
        public ICommand SelectPathToGameCommand => _SelectPathToGameCommand ??= new LambdaCommand(OnSelectPathToGameCommand);
        private async void OnSelectPathToGameCommand(object parameter)
        {
            var model = new SelectPathToGameViewModel();
            var result = await NotificationService.ShowDialog(model);
            if (result is ModernWpf.Controls.ContentDialogResult.None)
            {
                return;
            }
            PathToGame = model.Path;
        }
        #endregion

        #endregion

        #region User interface


        #region NavigationViewPaneDisplayMode
        public static NavigationViewPaneDisplayMode[] NavigationViewPaneDisplayModes =>
            Enum.GetValues<NavigationViewPaneDisplayMode>();

        public NavigationViewPaneDisplayMode NavigationViewPaneDisplayMode
        {
            get => Settings.Default.NavigationViewPaneDisplayMode;
            set
            {
                if (!Settings.Default.NavigationViewPaneDisplayMode.Equals(value))
                {
                    Settings.Default.NavigationViewPaneDisplayMode = value;
                    MainViewModel.NavigationViewPaneDisplayMode = value;
                    OnPropertyChanged(nameof(NavigationViewPaneDisplayMode));
                }
            }
        }
        #endregion

        #endregion

        public string ConfigPath => new FileInfo(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).DirectoryName;
    }
}
