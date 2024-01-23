using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models.Progress;
using Ethereal.FAF.UI.Client.Models.Settings;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class DownloadItemViewModel : Base.ViewModel
    {
        private readonly IDownloadService _downloadService;
        private readonly string _downloadUrl;
        private readonly string _downloadPath;

        public DownloadItemViewModel(IDownloadService downloadService, string downloadUrl, string targetFile, string file = null)
        {
            _downloadService = downloadService;
            _downloadUrl = downloadUrl;
            _downloadPath = targetFile;
            Name = file ?? Path.GetFileName(downloadUrl);
        }

        [ObservableProperty]
        private string _Name;
        [ObservableProperty]
        private string _Url;
        [ObservableProperty]
        private string _ProgressText;
        [ObservableProperty]
        private bool _IsIndeterminate;
        [ObservableProperty]
        private double _Progress;
        [ObservableProperty]
        private bool _Failed;
        [ObservableProperty]
        private bool _Finished;
        [ObservableProperty]
        private Exception _Exception;

        public async Task DownloadAsync()
        {
            Failed = false;
            Exception = null;
            bool extracting = false;
            var progress = new Progress<ProgressReport>(x =>
            {
                IsIndeterminate = x.IsIndeterminate;
                Progress = x.Progress ?? 0;
                if (extracting)
                {
                    ProgressText = $"({x.Percentage}%) " + x.Message;
                }
                else
                {
                    ProgressText = $"({x.Percentage}%) " + x.Message;
                }
            });
            try
            {
                await _downloadService.DownloadToFileAsync(_downloadUrl, _downloadPath, progress);
            }
            catch (Exception ex)
            {
                Failed = true;
                Exception = ex;
                return;
            }
            var ext = Path.GetExtension(_downloadPath);
            if (ext == ".rar")
            {
                await ArchiveHelper.Extract(_downloadPath, AppHelper.FilesDirectory.FullName, progress).ConfigureAwait(false);
                File.Delete(_downloadPath);
            }
            Finished = true;
            Progress = 100;
            ProgressText = "Downloaded";
            IsIndeterminate = false;
        }
    }
    public partial class PrepareClientViewModel : Base.ViewModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ISnackbarService _snackbarService;
        private readonly IDownloadService _downloadService;
        private readonly INavigationWindow _navigationWindow;

        public PrepareClientViewModel(ISettingsManager settingsManager, ISnackbarService snackbarService, IDownloadService downloadService, INavigationWindow navigationWindow)
        {
            _settingsManager = settingsManager;
            _snackbarService = snackbarService;
            _downloadService = downloadService;
            _navigationWindow = navigationWindow;
        }

        protected override void OnInitialLoaded()
        {
            base.OnInitialLoaded();
        }
        protected override async Task OnInitialLoadedAsync()
        {
            Loading = true;
            await _settingsManager.LoadAsync();
            Application.Current.Dispatcher.Invoke(() =>
            RequiredFiles = new(_settingsManager.ClientConfiguration.RequiredFiles));
            Loading = false;
        }


        [ObservableProperty]
        private ObservableCollection<RequiredFile> _RequiredFiles;

        [ObservableProperty]
        private bool _loading;
        [ObservableProperty]
        private int _CurrentStep = 1;
        [ObservableProperty]
        private int _StepsCount = 3;

        #region Stage - 1, Paths
        /// <summary>
        /// Location of original game
        /// </summary>
        [ObservableProperty]
        private string _ForgedAllianceLocation;
        /// <summary>
        /// Location of maps and mods
        /// </summary>
        [ObservableProperty]
        private string _ForgedAllianceVaultLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Gas Powered Games", "Supreme Commander Forged Alliance");
        /// <summary>
        /// Location for FAForever files
        /// </summary>
        [ObservableProperty]
        private string _FAForeverLocation = "C:\\ProgramData\\FAForever";

        [RelayCommand]
        private void SelectForgedAllianceLocation()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Select Supreme Commander: Forged Alliance executable",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "SupremeCommander.exe|SupremeCommander.exe"
            };
            if (dialog.ShowDialog() != true)
            {
                _snackbarService.Show("Warning", "File not selected", TimeSpan.FromSeconds(5));
                return;
            }
            var gameRootLocation = new FileInfo(dialog.FileName).Directory.Parent.FullName;
            if (!ForgedAllianceHelper.DirectoryHasAnyGameFile(gameRootLocation))
            {
                _snackbarService.Show("Warning", "Selected location missing root game files", TimeSpan.FromSeconds(5));
            }
            ForgedAllianceLocation = gameRootLocation;
            SaveStepOneAndContinueCommand.NotifyCanExecuteChanged();
        }
        [RelayCommand]
        private void SelectForgedAllianceVaultLocation()
        {
            var guid = Guid.NewGuid().ToString()[..5];
            var dialog = new OpenFileDialog()
            {
                Title = "Selected directory where you want to save Forged Alliance maps and mods",
                CheckFileExists = false,
                CheckPathExists = false,
                Filter = $"Any|*." + guid,
                FileName = "Press Enter or click Open." + guid
            };
            if (dialog.ShowDialog() != true)
            {
                _snackbarService.Show("Warning", "Directory not selected", TimeSpan.FromSeconds(5));
                return;
            }
            var directory = new DirectoryInfo(dialog.FileName);

            var folders = new string[] { "maps", "mods", "replays", "screenshots" };
            if (folders.Any(x => directory.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                directory = directory.Parent;
            }
            ForgedAllianceVaultLocation = new FileInfo(directory.FullName).DirectoryName;
            SaveStepOneAndContinueCommand.NotifyCanExecuteChanged();
        }
        [RelayCommand]
        private void SelectFAForeverLocation()
        {
            var guid = Guid.NewGuid().ToString()[..5];
            var dialog = new OpenFileDialog()
            {
                Title = "Selected directory where you want to save FAForever patche and etc",
                CheckFileExists = false,
                CheckPathExists = false,
                Filter = $"Any|*." + guid,
                FileName = "Press Enter or click Open." + guid
            };
            if (dialog.ShowDialog() != true)
            {
                _snackbarService.Show("Warning", "Directory not selected", TimeSpan.FromSeconds(5));
                return;
            }
            FAForeverLocation = new FileInfo(dialog.FileName).DirectoryName;
            SaveStepOneAndContinueCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanSaveStepOneAndContinue))]
        private void SaveStepOneAndContinue()
        {
            CurrentStep = 2;
            LoadFiles();
        }
        private bool CanSaveStepOneAndContinue() =>
            ForgedAllianceLocation != null
            && ForgedAllianceVaultLocation != null
            && FAForeverLocation != null;

        #endregion

        [RelayCommand(CanExecute = nameof(CanBackInStep))]
        private void BackInStep() => CurrentStep -= 1;
        private bool CanBackInStep() => CurrentStep > 2;

        #region Stage - 2, Files
        [ObservableProperty]
        private ObservableCollection<DownloadItemViewModel> _Downloads;

        private void LoadFiles()
        {
            var downloads = new ObservableCollection<DownloadItemViewModel>();
            foreach (var file in RequiredFiles)
            {
                var targetFile = Path.Combine(AppHelper.FilesDirectory.FullName, Path.GetFileName(file.Url));
                var model = new DownloadItemViewModel(_downloadService, file.Url, targetFile, file.Name);
                downloads.Add(model);
            }
            Downloads = downloads;
            Task.Run(async () =>
            {
                foreach (var download in Downloads)
                {
                    await download.DownloadAsync();
                }
            }).ContinueWith(x =>
            {
                if (x.IsCompletedSuccessfully)
                {
                    App.Current.Dispatcher.Invoke(() => _navigationWindow.Navigate(typeof(AuthView)));
                }
            }).SafeFireAndForget();
        }

        #endregion
    }
}
