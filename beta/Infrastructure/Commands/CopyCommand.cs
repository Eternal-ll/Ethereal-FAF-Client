using beta.Infrastructure.Commands.Base;
using System.Windows;

namespace beta.Infrastructure.Commands
{
    public class CopyCommand : Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;
            var text = string.Empty;
            if (parameter is string data) text = data;
            else text = parameter.ToString();
            Clipboard.SetText(text);    
        }
    }
}
