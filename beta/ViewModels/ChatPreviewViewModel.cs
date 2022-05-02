using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace beta.ViewModels
{
    public class ChatPreviewViewModel : Base.ViewModel
    {
        private readonly IIrcService IrcService;
        public ChatPreviewViewModel()
        {
            IrcService = App.Services.GetService<IIrcService>();

            IrcService.StateChanged += OnIrcStateChanged;
        }

        #region Properties

        public PlayersViewModel PlayersViewModel { get; } = new();

        #region IsAlwaysConnectToIRC
        private bool _IsAlwaysConnectToIRC = Settings.Default.ConnectIRC;
        public bool IsAlwaysConnectToIRC
        {
            get => _IsAlwaysConnectToIRC;
            set
            {
                if (Set(ref _IsAlwaysConnectToIRC, value))
                {
                    Settings.Default.ConnectIRC = value;
                }
            }
        }
        #endregion

        public bool IsRequestConnectBtnEnabled => !PendingConnectionToIRC;

        #region PendingConnectionToIRC
        private bool _PendingConnectionToIRC;
        public bool PendingConnectionToIRC
        {
            get => _PendingConnectionToIRC;
            set
            {
                if (Set(ref _PendingConnectionToIRC, value))
                {
                    OnPropertyChanged(nameof(IsRequestConnectBtnEnabled));
                }
            }
        }
        #endregion

        #endregion

        private void OnIrcStateChanged(object sender, IrcState e)
        {
            PendingConnectionToIRC = e != IrcState.Disconnected;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PlayersViewModel.Dispose();
                IrcService.StateChanged -= OnIrcStateChanged;
            }
        }
    }
}
