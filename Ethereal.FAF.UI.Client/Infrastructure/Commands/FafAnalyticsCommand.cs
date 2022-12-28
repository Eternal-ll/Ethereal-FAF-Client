namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    /// <summary>
    /// Also supports direct links for players like(all parameters are optional):
    /// https://kazbek.github.io/FAF-Analytics/player/{Login}/{GameType}
    /// https://kazbek.github.io/FAF-Analytics/player-id/{PlayerId}/{GameType}
    /// Game types: 1- Global + TMM, 2 - Ladder, 3 - TMM 2v2.
    /// Example ZLO:
    /// https://kazbek.github.io/FAF-Analytics/player/ZLO - ZLO Global
    /// https://kazbek.github.io/FAF-Analytics/player/ZLO/2 - ZLO Ladder
    /// https://kazbek.github.io/FAF-Analytics/player-id/145/3 - ZLO TMM 2v2
    /// </summary>
    internal class FafAnalyticsGlobalCommand : NagivateUriCommand
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter) => 
            base.Execute($"https://kazbek.github.io/FAF-Analytics/player-id/{parameter}/1");
    }
}