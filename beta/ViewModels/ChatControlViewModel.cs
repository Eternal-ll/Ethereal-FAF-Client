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

            UpdateState();

            IrcService.StateChanged += OnIrcStateChanged;

            if (Settings.Default.ConnectIRC && !IsConnected)
            {
                IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password);
            }
        }
        //private ChatPreviewViewModel ChatPreviewViewModel;
        //private ChatViewModel ChatViewModel;

        #region IsConnected
        private bool _IsConnected;
        public  bool IsConnected
        {
            get => _IsConnected;
            set
            {
                if (Set(ref _IsConnected, value))
                {
                    if (value)
                    {
                        CurrentViewModel = new ChatViewModel();
                    }
                    else
                    {
                        CurrentViewModel = new ChatPreviewViewModel();
                    }
                }
            }
        }
        #endregion

        #region CurrentViewModel
        private ViewModel _CurrentViewModel = new ChatPreviewViewModel();
        public ViewModel CurrentViewModel
        {
            get => _CurrentViewModel;
            set => Set(ref _CurrentViewModel, value);
        }
        #endregion

        private void UpdateState() => IsConnected = IrcService.State == IrcState.Authorized;

        private void OnIrcStateChanged(object sender, IrcState e) => UpdateState();
    }
}
