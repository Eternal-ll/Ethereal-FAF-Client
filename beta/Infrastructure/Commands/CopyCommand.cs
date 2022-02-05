using beta.Infrastructure.Commands.Base;
using System.Windows;

namespace beta.Infrastructure.Commands
{
    internal class CopyCommand : Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter) => Clipboard.SetText(parameter.ToString());
    }
}
