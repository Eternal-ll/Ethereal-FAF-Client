using beta.Infrastructure.Commands;
using System.Windows.Input;

namespace beta.ViewModels
{
    /// <summary>
    /// Dialog view model for passing password to the game
    /// </summary>
    internal class PassPasswordViewModel : Base.ViewModel
    {
        public PassPasswordViewModel()
        {

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

                }
            }
        }
        #endregion

        #region PassPasswordCommand
        private ICommand _PassPasswordCommand;
        public ICommand PassPasswordCommand => _PassPasswordCommand ??= new LambdaCommand(OnPassPasswordCommand);
        private void OnPassPasswordCommand(object parameter) { Password = parameter.ToString(); }

        #endregion
    }
}
