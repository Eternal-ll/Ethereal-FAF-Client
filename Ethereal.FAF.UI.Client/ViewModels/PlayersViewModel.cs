using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class PlayersViewModel : Base.ViewModel
    {
        public event EventHandler<Game> NewGameReceived;
        public event EventHandler<Game> GameUpdated;
        //public event EventHandler<Game> GameRemoved;

        public event EventHandler<Game> GameLaunched;
        public event EventHandler<Game> GameEnd;
        public event EventHandler<Game> GameClosed;

        public event EventHandler<string[]> PlayersLeftFromGame;
        public event EventHandler<KeyValuePair<Game, string[]>> PlayersJoinedToGame;

        public event EventHandler<KeyValuePair<Game, PlayerInfoMessage[]>> PlayersJoinedGame;
        public event EventHandler<KeyValuePair<Game, PlayerInfoMessage[]>> PlayersLeftGame;
        public event EventHandler<KeyValuePair<Game, PlayerInfoMessage[]>> PlayersFinishedGame;


        private readonly LobbyClient Lobby;

        public PlayersViewModel(LobbyClient lobby)
        {
            Lobby = lobby;
            lobby.PlayersReceived += Lobby_PlayersReceived;
            lobby.PlayerReceived += Lobby_PlayerReceived;
            lobby.WelcomeDataReceived += Lobby_WelcomeDataReceived;
        }

        public PlayerInfoMessage Self;

        private void Lobby_WelcomeDataReceived(object sender, WelcomeData e)
        {
            Self = e.me;
        }

        #region Players
        public CollectionViewSource PlayersSource { get; private set; }
        public ICollectionView PlayersView => PlayersSource?.View;
        private ObservableCollection<PlayerInfoMessage> _Players;
        public ObservableCollection<PlayerInfoMessage> Players
        {
            get => _Players;
            set
            {
                if (Set(ref _Players, value))
                {
                    PlayersSource = new()
                    {
                        Source = value
                    };
                    PlayersSource.Filter += PlayersSource_Filter;
                    OnPropertyChanged(nameof(PlayersView));
                }
            }
        }

        private void PlayersSource_Filter(object sender, FilterEventArgs e)
        {
            var player = (PlayerInfoMessage)e.Item;
            e.Accepted = false;
            e.Accepted = true;
        }
        #endregion

        private void Lobby_PlayerReceived(object sender, PlayerInfoMessage e)
        {
            if (e.Id == Self.Id)
            {
                Self = e;
            }
        }

        private void Lobby_PlayersReceived(object sender, PlayerInfoMessage[] e) =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                Players = new(e);
            }, DispatcherPriority.Background);
    }
}
    