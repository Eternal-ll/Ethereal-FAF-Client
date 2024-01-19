using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SelectFaPathLocationView.xaml
    /// </summary>
    public partial class SelectFaPatchLocationView : INavigableView<object>
    {
        private readonly LoaderViewModel LoaderViewModel;
        private readonly ISnackbarService SnackbarService;
        public SelectFaPatchLocationView(LoaderViewModel loaderViewModel, ISnackbarService snackbarService)
        {
            LoaderViewModel = loaderViewModel;
            InitializeComponent();
            SnackbarService = snackbarService;
        }
        private readonly object vm = new object();
        public object ViewModel => vm;

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var opnDlg = new OpenFileDialog()
            {
                Title = "Select folder where you want to save patch files and press Enter or click Open (c) Eternal. ☚ YOU, READ !",
                CheckFileExists = false,
                CheckPathExists = false,
                FileName = "Enter or Open"
            };
            if (opnDlg.ShowDialog() is false)
            {
                //SnackbarService.Show("Warning", "Dude, i gave you instructions and still you did something wrong =/", Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Caution);
                return;
            }
            var location = Path.GetDirectoryName(opnDlg.FileName);
            if (File.Exists(Path.Combine(location, FaPaths.DefaultConfigFile)))
            {
                //SnackbarService.Timeout = 10000;
                //SnackbarService.Show("Success", $"YAaaaa, my dear friend, thank you for using my client ♥. I`ll take care of this sacred place \"{location}\" =).", Wpf.Ui.Common.SymbolRegular.CheckboxChecked20, Wpf.Ui.Common.ControlAppearance.Success);
            }
            else
            {
                //SnackbarService.Timeout = 5000;
                //SnackbarService.Show("Success", $"I like it! It`s time to fill your directory \"{location}\" with some NSFW content!", Wpf.Ui.Common.SymbolRegular.CheckboxChecked20, Wpf.Ui.Common.ControlAppearance.Success);
            }
            UserSettings.Update(ConfigurationConstants.ForgedAlliancePatchLocation, location);
            Thread.Sleep(500);
            await LoaderViewModel.TryPassChecksAndLetsSelectServer();
        }
    }
}
