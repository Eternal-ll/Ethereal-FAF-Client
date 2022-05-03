using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace beta.Infrastructure.Commands
{
    internal class ConnectToIrcCommand : Base.Command
    {
        private readonly IIrcService IrcService;
        private readonly INotificationService NotificationService;
        public ConnectToIrcCommand()
        {
            IrcService = App.Services.GetService<IIrcService>();
            NotificationService = App.Services.GetService<INotificationService>();
        }
        public override bool CanExecute(object parameter) => IrcService.State == Models.Enums.IrcState.Disconnected;
        public override void Execute(object parameter)
        {
            Task.Run(() => IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password))
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        NotificationService.ShowExceptionAsync(task.Exception);
                    }
                });
        }
    }
    internal class DisconnectFromIrcCommand : Base.Command
    {
        private readonly IIrcService IrcService;
        public DisconnectFromIrcCommand() => IrcService = App.Services.GetService<IIrcService>();
        public override bool CanExecute(object parameter) => IrcService.State == Models.Enums.IrcState.Authorized;
        public override void Execute(object parameter) => IrcService.Quit();
    }
    internal class RefreshIrcCommand : Base.Command
    {
        private readonly IIrcService IrcService;
        public RefreshIrcCommand() => IrcService = App.Services.GetService<IIrcService>();
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter) => IrcService.Restart(Settings.Default.PlayerNick, Settings.Default.irc_password);
    }
}
