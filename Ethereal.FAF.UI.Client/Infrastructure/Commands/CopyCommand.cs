using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal sealed class CopyCommand : Base.Command
    {
        private ISnackbarService _snackbarService { get; set; }
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;
            _snackbarService ??= App.Hosting.Services.GetService<ISnackbarService>();
            Clipboard.SetText(parameter.ToString());
            _snackbarService.Show("Clipboard", $"\"{parameter}\" copied to clipboard!", Wpf.Ui.Controls.ControlAppearance.Primary, null, TimeSpan.FromSeconds(3));
        }
    }
}
