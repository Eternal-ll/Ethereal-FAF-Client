using AsyncAwaitBestPractices;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Ethereal.FAF.UI.Client.ViewModels.Dialogs;
using System.Windows;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for UpdateClientView.xaml
    /// </summary>
    public partial class UpdateClientView : INavigableView<UpdateViewModel>
    {
        public UpdateClientView(UpdateViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public UpdateViewModel ViewModel { get; }

        public override ViewModel GetViewModel() => ViewModel;

        protected override void OnLoadedEvent(RoutedEventArgs e)
        {
            base.OnLoadedEvent(e);
            ViewModel.InstallUpdateCommand.ExecuteAsync(null).SafeFireAndForget();
        }
    }
}
