using beta.Properties;

namespace beta.Infrastructure.Commands
{
    internal class LogoutCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            Settings.Default.PlayerId = 0;
            Settings.Default.PlayerNick = null;
            Settings.Default.access_token = null;
            Settings.Default.refresh_token = null;
            Settings.Default.id_token = null;
            Settings.Default.AutoJoin = false;
            App.Restart();
        }
    }
}
