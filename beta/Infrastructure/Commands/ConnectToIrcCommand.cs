using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace beta.Infrastructure.Commands
{
    public class ConnectToIrcCommand : Base.Command
    {
        private IIrcService IrcService;
        private INotificationService NotificationService;
        public override bool CanExecute(object parameter) => (IrcService ??= App.Services.GetService<IIrcService>())
            .State == Models.Enums.IrcState.Disconnected;
        public override void Execute(object parameter) => 
            Task.Run(() => (IrcService ??= ServiceProvider.GetService<IIrcService>()).Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password))
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        (NotificationService ??= ServiceProvider.GetService<INotificationService>()).ShowExceptionAsync(task.Exception);
                    }
                });
    }
    public class DisconnectFromIrcCommand : Base.Command
    {
        private IIrcService IrcService;
        public override bool CanExecute(object parameter) => (IrcService ??= ServiceProvider.GetService<IIrcService>()).State == Models.Enums.IrcState.Authorized;
        public override void Execute(object parameter) => (IrcService ??= ServiceProvider.GetService<IIrcService>()).Quit();
    }
    internal class RefreshIrcCommand : Base.Command
    {
        private IIrcService IrcService;
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter) => (IrcService ??= ServiceProvider.GetService<IIrcService>()).Restart(Settings.Default.PlayerNick, Settings.Default.irc_password);
    }
}
