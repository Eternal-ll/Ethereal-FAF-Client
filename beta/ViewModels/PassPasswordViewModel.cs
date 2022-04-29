using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System.Windows.Input;

namespace beta.ViewModels
{
    /// <summary>
    /// Dialog view model for passing password to the game
    /// </summary>
    internal class PassPasswordViewModel : Base.ViewModel
    {
        private readonly ContentDialog ContentDialog;

        public PassPasswordViewModel()
        {
            ContentDialog = App.Services.GetService<INotificationService>().ContentDialog;
        }

        #region Password
        private string _Password;
        public string Password
        {
            get => _Password;
            set
            {
                if (Set(ref _Password, value))
                {
                    ContentDialog.IsPrimaryButtonEnabled = !string.IsNullOrEmpty(value);
                }
            }
        }
        #endregion

        #region PassPasswordCommand
        private ICommand _PassPasswordCommand;
        public ICommand PassPasswordCommand => _PassPasswordCommand ??= new LambdaCommand(OnPassPasswordCommand);
        private void OnPassPasswordCommand(object parameter) {}

        #endregion
    }
}
