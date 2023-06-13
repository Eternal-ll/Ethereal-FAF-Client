using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models.UsersController;
using MediatR;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.Infrastructure.Mediator
{
    internal record ApiNotification(string Title, string Detail, bool Success = true) : INotification;
    internal class SnackBarNotification : INotificationHandler<ApiNotification>
    {
        private readonly ISnackbarService _snackbarService;

        public SnackBarNotification(ISnackbarService snackbarService)
        {
            _snackbarService = snackbarService;
        }

        public async Task Handle(ApiNotification notification, CancellationToken cancellationToken)
        {
            var appearence = notification.Success ? ControlAppearance.Success : ControlAppearance.Caution;
            await _snackbarService.ShowAsync(notification.Title, notification.Detail, Wpf.Ui.Common.SymbolRegular.Empty, appearence);
        }
    }
    internal class ChangeEmailHandler : IRequestHandler<ChangeEmailCommand, bool>
    {
        private readonly IFafUserService _fafUserService;
        private readonly IMediator _mediator;

        public ChangeEmailHandler(IFafUserService fafUserService, IMediator mediator)
        {
            _fafUserService = fafUserService;
            _mediator = mediator;
        }

        public async Task<bool> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
        {
            var response = await _fafUserService.ChangeEmail(request.NewEmail, request.Password, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadFromJsonAsync<RequestError>();
                var error = problem.Errors[0];
                await _mediator.Publish(new ApiNotification(error.Title, $"{error.Status} {error.Code} {error.Detail}", false), cancellationToken);
            }
            else
            {
                await _mediator.Publish(new ApiNotification("Success", "Email succesfully changed"), cancellationToken);
            }
            return response.IsSuccessStatusCode;
        }
    }
}
