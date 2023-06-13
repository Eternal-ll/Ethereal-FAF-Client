using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.Infrastructure.Mediator
{
    internal class NotifyCommandHandler : IRequestHandler<NotifyCommand>
    {
        private readonly ISnackbarService _snackbarService;

        public NotifyCommandHandler(ISnackbarService snackbarService)
        {
            _snackbarService = snackbarService;
        }

        public Task Handle(NotifyCommand request, CancellationToken cancellationToken)
        {
            _snackbarService.Show(request.Title, request.Message, request.Icon, request.Appearance);
            return Task.CompletedTask;
        }
    }
}
