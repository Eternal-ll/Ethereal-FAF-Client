using beta.Infrastructure.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace beta.ViewModels
{
    public abstract class ApiViewModel : Base.ViewModel
    {
        public event EventHandler RequestFinished;

        #region Id
        private int _Id = -1;
        /// <summary>
        /// Entity id
        /// </summary>
        public int Id
        {
            get => _Id;
            set
            {
                if (Set(ref _Id, value))
                {
                    RunRequest();
                }
            }
        }
        #endregion

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
                    OnPropertyChanged(nameof(InputVisibility));
                }
            }
        }
        #endregion

        public Visibility InputVisibility => IsPendingRequest ? Visibility.Hidden : Visibility.Visible;
        public bool IsInputEnabled => !IsPendingRequest;

        public async Task DoRequestAsync()
        {
            if (IsPendingRequest) return;
            IsPendingRequest = true;
            await RequestTask().ContinueWith(task =>
            {
                if (task.IsFaulted) IsPendingRequest = false;
            });
            IsPendingRequest = false;
            if (IsRefreshing) IsRefreshing = false;
            RequestFinished?.Invoke(this, null);
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
