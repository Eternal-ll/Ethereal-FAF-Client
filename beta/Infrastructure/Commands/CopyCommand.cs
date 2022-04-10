using beta.Infrastructure.Commands.Base;
using System.Windows;

namespace beta.Infrastructure.Commands
{
    internal class CopyCommand : Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;

            Clipboard.SetText(parameter.ToString());
        }
    }
}
