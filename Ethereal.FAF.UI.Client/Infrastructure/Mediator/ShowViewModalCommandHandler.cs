using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Views;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.Infrastructure.Mediator
{
    internal class ShowViewModalCommandHandler : 
        IRequestHandler<ShowChangeEmailModalCommand>
    {
        private readonly IDialogService _dialogService;
        private readonly ChangeEmailView _view;

        public ShowViewModalCommandHandler(IDialogService dialogService, ChangeEmailView view)
        {
            _dialogService = dialogService;
            _view = view;
        }

        public Task Handle(ShowChangeEmailModalCommand request, CancellationToken cancellationToken)
        {
            var dialog = _dialogService.GetDialogControl();
            var internalDialog = (Dialog)dialog;

            if (dialog.IsShown) dialog.Hide();

            dialog.Title = null;
            dialog.Footer = null;

            dialog.ButtonLeftName = "Update";
            dialog.ButtonRightName = "Hide";

            internalDialog.ButtonLeftVisibility = System.Windows.Visibility.Collapsed;
            internalDialog.ButtonRightVisibility = System.Windows.Visibility.Visible;

            internalDialog.ButtonLeftAppearance = Wpf.Ui.Common.ControlAppearance.Success;
            internalDialog.ButtonRightAppearance = Wpf.Ui.Common.ControlAppearance.Secondary;

            _view.ViewModel.ShowUpdateButton = new LambdaCommand((object arg) => internalDialog.ButtonLeftVisibility = System.Windows.Visibility.Visible);
            _view.ViewModel.HideUpdateButton = new LambdaCommand((object arg) => internalDialog.ButtonLeftVisibility = System.Windows.Visibility.Collapsed);

            internalDialog.ButtonLeftClick += InternalDialog_ButtonLeftClick;

            dialog.Content = _view;
            dialog.Show();
            dialog.Closed += Dialog_Closed;
            return Task.CompletedTask;
        }

        private async void InternalDialog_ButtonLeftClick(object sender, System.Windows.RoutedEventArgs e)
        {
            e.Handled = !await _view.ViewModel.ChangeEmail();
        }

        private void Dialog_Closed(Wpf.Ui.Controls.Dialog sender, System.Windows.RoutedEventArgs e)
        {
            sender.ButtonLeftClick -= InternalDialog_ButtonLeftClick;
            sender.Closed -= Dialog_Closed;
            sender.Content = null;
            sender.Footer = null;
        }
    }
}
