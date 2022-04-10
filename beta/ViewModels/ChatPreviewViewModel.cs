using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Windows.Data;

namespace beta.ViewModels
{
    public class ChatPreviewViewModel : Base.ViewModel
    {
        private readonly IIrcService IrcService;
        private readonly IPlayersService PlayersService;
        public ChatPreviewViewModel()
        {
            IrcService = App.Services.GetService<IIrcService>();
            PlayersService = App.Services.GetService<IPlayersService>();

            IrcService.StateChanged += OnIrcStateChanged;

            //BindingOperations.EnableCollectionSynchronization(PlayersService.Players, new object());
            OnlinePlayersViewSource.Filter += PlayersFilter;
            OnlinePlayersViewSource.Source = PlayersService.Players;
        }

        #region Properties

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

        #region FilterText
        private string _FilterText = string.Empty;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    OnlinePlayersView.Refresh();
                }
            }
        }
        #endregion

        #region Online players
        private readonly CollectionViewSource OnlinePlayersViewSource = new();
        public ICollectionView OnlinePlayersView => OnlinePlayersViewSource.View;
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

        private void PlayersFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;
            var filter = FilterText;
            if (string.IsNullOrWhiteSpace(filter)) return;

            var player = (IPlayer)e.Item;
            e.Accepted = player.login.StartsWith(filter, StringComparison.OrdinalIgnoreCase);

        }

        private void OnIrcStateChanged(object sender, IrcState e) =>
            PendingConnectionToIRC = e != IrcState.Disconnected;
    }
}
