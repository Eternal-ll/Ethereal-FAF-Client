using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Win32;
using System.IO;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SelectGameLocation.xaml
    /// </summary>
    public partial class SelectGameLocationView : INavigableView<object>
    {
        private readonly LoaderViewModel LoaderViewModel;
        private readonly SnackbarService SnackbarService;
        public SelectGameLocationView(LoaderViewModel loaderVM, SnackbarService snackbarService)
        {
            LoaderViewModel = loaderVM;
            InitializeComponent();
            SnackbarService = snackbarService;
        }
        private readonly object vm = new object();
        public object ViewModel => vm;


        private string LastLocation;
        private async void SelectForgedAllianceExecutable(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Give me location of your Supreme Commander: Forged Alliance executable. ☚ YOU, READ !",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = LastLocation ?? "C:\\",
                Filter = "SCFA|SupremeCommander.exe|Executable (*.exe)|*.exe"
            };
            if (dialog.ShowDialog() is false)
            {
                return;
            }
            var selectedFile = dialog.FileName;
            var directory = Path.GetDirectoryName(Path.GetDirectoryName(selectedFile));
            LastLocation = directory;
            SnackbarService.Timeout = 5000;
            if (File.Exists(Path.Combine(directory, FaPaths.DefaultConfigFile)))
            {
                SnackbarService.Show("Warning", "You little sly, you must select executable from original game folder.", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Caution);
                return;
            }
            if (!ForgedAllianceHelper.DirectoryHasAnyGameFile(directory))
            {
                SnackbarService.Show("Warning", $"Yo, your location \"{directory}\" failed validation for required files.", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Caution);
                return;
            }
            SnackbarService.Show("Success", $"There we go! Directory \"{directory}\" will be used as source for original game files.", Wpf.Ui.Common.SymbolRegular.CheckboxChecked20, Wpf.Ui.Common.ControlAppearance.Success);
            UserSettings.Update(ConfigurationConstants.ForgedAllianceLocation, directory);
            await LoaderViewModel.TryPassChecksAndLetsSelectServer();
        }
    }
}
