using System.Diagnostics;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class NagivateUriCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = parameter.ToString();
            process.Start();
        }
    }
}