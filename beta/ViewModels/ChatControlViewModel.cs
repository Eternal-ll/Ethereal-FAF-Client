using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace beta.ViewModels
{
    public class ChatControlViewModel : ViewModel
    {
        private readonly IIrcService IrcService;

        public ChatControlViewModel()
        {
            IrcService = App.Services.GetService<IIrcService>();

            if (Settings.Default.ConnectIRC)
            {

            }
            else
            {
                CurrentViewModel = new ChatPreviewViewModel();
            }
            IrcService.StateChanged += OnIrcStateChanged;
        }

        #region IsConnected
        private bool _IsConnected;
        public  bool IsConnected
        {
            get => _IsConnected;
            set
            {
                if (Set(ref _IsConnected, value))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (value)
                        {
                            CurrentViewModel = new ChatViewModel();
                        }
                        else
                        {
                            CurrentViewModel = new ChatPreviewViewModel();
                        }
                    });
                }
            }
        }
        #endregion

        #region CurrentViewModel
        private ViewModel _CurrentViewModel;
        public ViewModel CurrentViewModel
        {
            get => _CurrentViewModel;
            set
            {
                if (_CurrentViewModel is not null)
                {
                    _CurrentViewModel.Dispose();
                }
                if (Set(ref _CurrentViewModel, value))
                {

                }
            }
        }
        #endregion

        private void UpdateState() => IsConnected = IrcService.State == IrcState.Authorized;

        private void OnIrcStateChanged(object sender, IrcState e)
        {
            if (e == IrcState.PendingConnection || e == IrcState.PendingAuthorization)
            {
                CurrentViewModel = new ChatConnectingViewModel();
                return;
            }
            UpdateState();
        }
    }
}
