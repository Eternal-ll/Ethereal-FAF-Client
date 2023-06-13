using MediatR;

namespace Ethereal.FAF.UI.Client.Infrastructure.Mediator
{
    internal record ChangeEmailCommand(string NewEmail, string Password) : IRequest<bool>;
}
