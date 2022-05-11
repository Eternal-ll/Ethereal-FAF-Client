using beta.Infrastructure.Commands;
using System.Threading.Tasks;
using System.Windows.Input;

namespace beta.ViewModels
{
    public abstract class ApiViewModel : Base.ViewModel
    {
        protected bool IsRefreshing = false;

        #region IsPendingRequest
        private bool _IsPendingRequest;
        public bool IsPendingRequest
        {
            get => _IsPendingRequest;
            set
            {
                if (Set(ref _IsPendingRequest, value))
                {
                    OnPropertyChanged(nameof(IsInputEnabled));
                }
            }
        }
        #endregion

        public bool IsInputEnabled => !IsPendingRequest;

        public async Task DoRequestAsync()
        {
            IsPendingRequest = true;
            await RequestTask();
            IsPendingRequest = false;
            if (IsRefreshing) IsRefreshing = false;
        }

        public void RunRequest() => Task.Run(() => DoRequestAsync());

        protected abstract Task RequestTask();

        #region RefreshCommand
        private ICommand _RefreshCommand;
        public ICommand RefreshCommand => _RefreshCommand ??= new LambdaCommand(OnRefreshCommand, CanRefreshCommand);
        private bool CanRefreshCommand(object parameter) => !IsPendingRequest;
        protected virtual void OnRefreshCommand(object parameter)
        {
            IsRefreshing = true;
            RunRequest();
        }
        #endregion
    }
}
