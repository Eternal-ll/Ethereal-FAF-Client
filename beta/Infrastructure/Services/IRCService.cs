using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.ViewModels.Base;
using beta.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Data;

namespace beta.Infrastructure.Services
{
    // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!
    public class IrcService : ViewModel, IIrcService
    {
        #region Events
        public event EventHandler<EventArgs<ChannelMessage>> Message;
        #endregion

        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayersService;

        #endregion

        #region FilterText

        private string _FilterText = string.Empty;

        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                    ChannelUsersView.Refresh();
            }
        }
        #endregion

        public ObservableCollection<Channel> Channels { get; } = new();

        private Channel _SelectedChannel;
        public Channel SelectedChannel
        {
            get => _SelectedChannel;
            set => Set(ref _SelectedChannel, value);
        }

        #region ChannelUsers

        private readonly CollectionViewSource ChannelUsersViewSource = new();
        public ICollectionView ChannelUsersView => ChannelUsersViewSource.View;

        #endregion

        private object _lock;
        
        #endregion

        public IrcService(
            ISessionService sessionService,
            IPlayersService playersService)
        {
            SessionService = sessionService;
            PlayersService = playersService;


            ChannelUsersViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PlayerInfoMessage.IsChatModerator)));
            ChannelUsersViewSource.SortDescriptions.Add(new(nameof(PlayerInfoMessage.IsChatModerator), ListSortDirection.Descending));
            ChannelUsersViewSource.SortDescriptions.Add(new(nameof(PlayerInfoMessage.RelationShip), ListSortDirection.Descending));

            ChannelUsersViewSource.Filter += ChannelUsersViewSourceFilter;

            App.Current.Dispatcher.Invoke(() => _lock = new());

            BindingOperations.EnableCollectionSynchronization(playersService.Players, _lock);
            
            ChannelUsersViewSource.Source = PlayersService.Players;
        }

        private void ChannelUsersViewSourceFilter(object sender, FilterEventArgs e)
        {
            var player = (PlayerInfoMessage)sender;
            e.Accepted = true;
        }

        public string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.AppendFormat("{0:x2}", hashBytes[i]);
            }
            return sb.ToString();
        }
    }
}
