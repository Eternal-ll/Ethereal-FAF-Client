namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class FafAnalyticsLadderCommand : NagivateUriCommand
    {
        public override void Execute(object parameter) => 
            base.Execute($"https://kazbek.github.io/FAF-Analytics/player-id/{parameter}/2");
    }
}