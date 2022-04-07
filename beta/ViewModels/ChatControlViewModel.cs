using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
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
        }
        private ChatPreviewViewModel ChatPreviewViewModel;
        private ChatViewModel ChatViewModel;

        #region IsConnected
        private bool _IsConnected;
        public  bool IsConnected
        {
            get => _IsConnected;
            set
            {
                if (Set(ref _IsConnected, value))
                {
                }
                if (value)
                {
                    CurrentViewModel = ChatViewModel ??= new();
                }
                else
                {
                    CurrentViewModel = ChatPreviewViewModel ??= new();
                }
            }
        }
        #endregion

        #region CurrentViewModel
        private ViewModel _CurrentViewModel;
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
