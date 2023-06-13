using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Mediator;
using MediatR;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ProfileViewModel : Base.ViewModel
    {
        private readonly IMediator Mediator;

        public ProfileViewModel(IMediator mediator, IDialogService dialogService)
        {
            Mediator = mediator;

            ShowChangeEmailModalCommand = new LambdaCommand(async (object arg) => await Mediator.Send(new ShowChangeEmailModalCommand()));
        }

        public ICommand ShowChangeEmailModalCommand { get; }
    }
}
