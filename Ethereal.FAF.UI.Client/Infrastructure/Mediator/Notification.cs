using MediatR;
using Wpf.Ui.Common;

namespace Ethereal.FAF.UI.Client.Infrastructure.Mediator
{
    internal record NotifyCommand(string Title, string Message, SymbolRegular Icon = SymbolRegular.Empty, ControlAppearance Appearance = ControlAppearance.Secondary)
        : IRequest;
}
