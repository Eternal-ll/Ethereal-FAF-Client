using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal sealed class CopyCommand : Base.Command
    {
        private NotificationService Notification { get; set; }
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;
            Notification ??= App.Hosting.Services.GetService<NotificationService>();
            Clipboard.SetText(parameter.ToString());
            Notification.Notify("Copied", parameter.ToString(), Wpf.Ui.Common.SymbolRegular.Copy24);
        }
    }
}
