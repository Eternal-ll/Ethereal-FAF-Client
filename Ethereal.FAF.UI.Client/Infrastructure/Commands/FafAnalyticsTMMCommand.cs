namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class FafAnalyticsTMMCommand : NagivateUriCommand
    {
        public override void Execute(object parameter) => 
            base.Execute($"https://kazbek.github.io/FAF-Analytics/player-id/{parameter}/3");
    }
}