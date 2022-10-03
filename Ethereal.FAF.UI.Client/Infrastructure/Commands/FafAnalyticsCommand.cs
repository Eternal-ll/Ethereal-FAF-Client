using System.Diagnostics;

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
    internal class FafAnalyticsGlobalCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"https://kazbek.github.io/FAF-Analytics/player-id/{parameter}/1";
            process.Start();
        }
    }
    internal class FafAnalyticsLadderCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"https://kazbek.github.io/FAF-Analytics/player-id/{parameter}/2";
            process.Start();
        }
    }
    internal class FafAnalyticsTMMCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"https://kazbek.github.io/FAF-Analytics/player-id/{parameter}/3";
            process.Start();
        }
    }
    /// <summary>
    /// https://fafscore.nl/{login}
    /// </summary>
    internal class FafScoreCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = $"https://fafscore.nl/{parameter}";
            process.Start();
        }
    }

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