namespace beta.Infrastructure.Commands
{
    internal class RestartCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter) => App.Restart();
    }
}
