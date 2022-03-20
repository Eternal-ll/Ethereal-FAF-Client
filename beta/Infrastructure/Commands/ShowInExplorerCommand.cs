using System.Diagnostics;

namespace beta.Infrastructure.Commands
{
    internal class ShowInExplorerCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter) => Process.Start("explorer.exe", parameter.ToString());
    }
}
