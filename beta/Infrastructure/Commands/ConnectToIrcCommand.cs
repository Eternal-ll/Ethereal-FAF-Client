using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    internal class ConnectToIrcCommand : Base.Command
    {
        private readonly IIrcService IrcService;
        public ConnectToIrcCommand() => IrcService = App.Services.GetService<IIrcService>();
        public override bool CanExecute(object parameter) => IrcService.State == Models.Enums.IrcState.Disconnected;
        public override void Execute(object parameter) => IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password);
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
