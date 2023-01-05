using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SelectVaultLocationView.xaml
    /// </summary>
    public partial class SelectVaultLocationView : INavigableView<object>
    {
        private readonly LoaderViewModel LoaderViewModel;
        private readonly SnackbarService SnackbarService;
        public SelectVaultLocationView(LoaderViewModel loaderViewModel, SnackbarService snackbarService)
        {
            InitializeComponent();
            LoaderViewModel = loaderViewModel;
            SnackbarService = snackbarService;
        }
        private readonly object vm = new object();
        public object ViewModel => vm;

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var opnDlg = new OpenFileDialog()
            {
                Title = "Select folder where you want to save maps and sim/ui mods files and press Enter or click Open (c) Eternal. ☚ YOU, READ !",
                CheckFileExists = false,
                CheckPathExists = false,
                InitialDirectory = Environment.ExpandEnvironmentVariables(ForgedAllianceHelper.DefaultVaultLocation),
                FileName = "Enter or Open"
            };
            if (opnDlg.ShowDialog() is false)
            {
                SnackbarService.Show("Warning", "Dude, i gave you instructions and still you did something wrong =/", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Caution);
                return;
            }
            var location = Path.GetDirectoryName(opnDlg.FileName);
            if (File.Exists(Path.Combine(location, "maps")) || File.Exists(Path.Combine(location, "mods")))
            {
                SnackbarService.Timeout = 10000;
                SnackbarService.Show("Success", $"Wow, i see there something! It`s time to clear this place \"{location}\" =).", Wpf.Ui.Common.SymbolRegular.CheckboxChecked20, Wpf.Ui.Common.ControlAppearance.Success);
            }
            else
            {
                SnackbarService.Timeout = 5000;
                SnackbarService.Show("Success", $"Pretty loose! It`s time to fill this place \"{location}\" with some huge maps and mods!", Wpf.Ui.Common.SymbolRegular.CheckboxChecked20, Wpf.Ui.Common.ControlAppearance.Success);
            }
            UserSettings.Update(ConfigurationConstants.ForgedAllianceVaultLocation, location);
            Thread.Sleep(500);
            await LoaderViewModel.TryPassChecksAndLetsSelectServer();
        }
    }
}
