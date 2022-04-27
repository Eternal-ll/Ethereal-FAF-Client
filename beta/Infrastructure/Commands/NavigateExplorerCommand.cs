using beta.Infrastructure.Commands.Base;
using beta.Properties;
using System;
using System.Diagnostics;

namespace beta.Infrastructure.Commands
{
    internal class NavigateExplorerCommand : Command
    {
        public override bool CanExecute(object parameter) => parameter is not null;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;

            var args = string.Empty;

            var user = Environment.UserName;

            switch (parameter.ToString().ToLower())
            {
                case "maps":
                    args = " " + Settings.Default.PathToMaps;
                    break;
                case "mods":
                    args = " " + Settings.Default.PathToMods;
                    break;
                case "game":
                    args = Settings.Default.PathToGame;
                    break;
                case "patch":
                    return;
                    break;
                default:
                    args = parameter.ToString();
                    break;
            }

            args = args.Replace("%username%", user);

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = args,
                UseShellExecute = true,
            });
        }
    }
}
