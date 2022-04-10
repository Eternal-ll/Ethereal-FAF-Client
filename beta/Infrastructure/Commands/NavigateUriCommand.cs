using beta.Infrastructure.Commands.Base;
using System;
using System.Diagnostics;

namespace beta.Infrastructure.Commands
{
    internal class NavigateUriCommand : Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;

            Uri uri = parameter as Uri;

            if (parameter is string text)
                Uri.TryCreate(text, UriKind.Absolute, out uri);

            if (uri is not null)
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri.ToString(),
                    UseShellExecute = true,
                });
        }
    }
}
