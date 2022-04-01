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

        #region IsConnected
        private bool _IsConnected;
        public  bool IsConnected
        {
            get
            {
                if (_IsConnected)
                {

                }
                return _IsConnected; ;
            }
            set => Set(ref _IsConnected, value);
        }
        #endregion

        private void UpdateState() => IsConnected = IrcService.State == IrcState.Authorized;

        private void OnIrcStateChanged(object sender, IrcState e) => UpdateState();
    }
}
