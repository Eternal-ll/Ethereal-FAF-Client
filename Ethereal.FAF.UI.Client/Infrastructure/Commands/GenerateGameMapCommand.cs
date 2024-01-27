using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class GenerateGameMapCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => false;
        public override void Execute(object parameter) => throw new NotImplementedException();
    }
}
