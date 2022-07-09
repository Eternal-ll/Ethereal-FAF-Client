using System.Windows;

namespace beta.Infrastructure.Commands
{
    internal class RestartCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location[..^3] + "exe");
            Application.Current.Shutdown();
        }
    }
}
