using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameInfoMessage = beta.Models.Server.GameInfoMessage;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for LobbiesView.xaml
    /// </summary>
    public partial class LobbiesView : INotifyPropertyChanged
    {
        private readonly ILobbySessionService _LobbySessionService;

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region Lobbies
        private readonly CollectionViewSource CollectionViewSource = new();
        private readonly ObservableCollection<GameInfoMessage> Lobbies = new();
        public ICollectionView View => CollectionViewSource.View;

        #endregion

        #region CTOR
        public LobbiesView()
        {
            InitializeComponent();
            CollectionViewSource.Filter += LobbiesViewSourceFilter;

            var grouping = new PropertyGroupDescription(nameof(GameInfoMessage.featured_mod));
            CollectionViewSource.GroupDescriptions.Add(grouping);

            _LobbySessionService = App.Services.GetRequiredService<ILobbySessionService>();

            _LobbySessionService.NewGameInfo += OnNewGameInfo;
            _LobbySessionService.UpdateGameInfo += OnUpdateGameInfo;

            for (int i = 0; i < _LobbySessionService.Games.Count; i++)
            {
                var lobby = _LobbySessionService.Games[i];
                if (lobby.num_players == 0 || lobby.launched_at != null) continue;
                Lobbies.Add(_LobbySessionService.Games[i]);
            }
            CollectionViewSource.Source = Lobbies;
            DataContext = this;
        }
        #endregion

        #region SearchText

        private string _SearchText = string.Empty;

        public string SearchText
        {
            get => _SearchText;
            set
            {
                _SearchText = value;
                OnPropertyChanged(nameof(SearchText));
                View.Refresh();
            }
        }
        #endregion

        #region IsPrivateGamesHidden

        private bool _IsPrivateGamesHidden;

        public bool IsPrivateGamesHidden
        {
            get => _IsPrivateGamesHidden;
            set
            {
                _IsPrivateGamesHidden = value;
                View.Refresh();
                OnPropertyChanged(nameof(IsPrivateGamesHidden));
            }
        }
        #endregion

        #region IsGameModeFilterEnabled
        private bool _IsGameModeFilterEnabled;
        public bool IsGameModeFilterEnabled
        {
            get => _IsGameModeFilterEnabled;
            set
            {
                _IsGameModeFilterEnabled = value;

                OnPropertyChanged(nameof(IsGameModeFilterEnabled));
            }
        } 
        #endregion

        #region SelectedGameModes codes
        //  3 - faf
        //  4 - coop
        //  5 - koth
        //  6 - nomads
        //  7 - fafbeta
        //  8 - phantomx
        //  9 - labwars
        // 10 - fafdevelop
        // 11 - murderparty
        // 12 - xtremewars
        // 14 - claustrophobia 
        #endregion

        #region LobbiesViewSourceFilter
        private void LobbiesViewSourceFilter(object sender, FilterEventArgs e)
        {
            string searchText = _SearchText;
            var lobby = (GameInfoMessage)e.Item;
            
            if (IsPrivateGamesHidden)
            {
                if (lobby.password_protected)
                {
                    e.Accepted = false;
                    return;
                }
            }
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                e.Accepted = lobby.title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)||
                             lobby.host.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
            }
        } 
        #endregion

        private void OnUpdateGameInfo(object sender, Infrastructure.EventArgs<GameInfoMessage> e)
        {
            Dispatcher.Invoke(() =>
            {
                var lobby = e.Arg;
                
                var mapName = lobby.MapName;
                if (mapName.Contains("gap") || mapName.Contains("crater") || mapName.Contains("astro") ||
                    mapName.Contains("pass"))
                    return;
                
                
                if (lobby.game_type.Length == 10) //matchmaker
                    return;

                if (lobby.num_players == 0)
                    return;

                for (int i = 0; i < Lobbies.Count; i++)
                {
                    var cLobby = Lobbies[i];

                    if (cLobby.uid == lobby.uid)
                    {

                        if (lobby.launched_at != null)
                        {
                            Lobbies.RemoveAt(i);
                            return;
                        }
                        Lobbies[i] = lobby;
                        return;
                    }                    
                }
                if (lobby.launched_at == null)
                    Lobbies.Add(lobby);
            });
        }
        private void OnNewGameInfo(object sender, Infrastructure.EventArgs<GameInfoMessage> e)
        {
            Dispatcher.Invoke(() =>
            {
                var lobby = e.Arg;

                var mapName = lobby.MapName;
                if (mapName.Contains("gap") || mapName.Contains("crater") || mapName.Contains("astro") ||
                    mapName.Contains("pass"))
                    return;
                

                if (lobby.game_type.Length == 10) //matchmaker
                    return;

                if (lobby.launched_at != null)
                    return;

                if (lobby.num_players == 0)
                    return;

                Lobbies.Add(lobby);
            });
        }
        
        //private int GetGameModeIndex(int mode)
        //{
        //    //    3 => "FAF",
        //    //    4 => mod[0] == 'c' ? "Coop" : "King of the Hill",//koth
        //    //    6 => "Nomads",
        //    //    7 => mod[0] == 'f' ? "FAF Beta" : "LabWars", //fafbeta
        //    //    8 => "Phantom-X",
        //    //    10 => mod[0] == 'f' ? "FAF Develop": "Extreme Wars",
        //    //    11 => "Murder Party",
        //    //    14 => "Claustrophobia",
        //    //    _ => "Unknown"

        //    //  3 - faf
        //    //  4 - coop
        //    //  5 - koth
        //    //  6 - nomads
        //    //  7 - fafbeta
        //    //  8 - phantomx
        //    //  9 - labwars
        //    // 10 - fafdevelop
        //    // 11 - murderparty
        //    // 12 - xtremewars
        //    // 14 - claustrophobia
        //    int index = mode.Length;
        //    return mode switch
        //    {
        //        4 when mode[0] != 'c' => 5,
        //        7 when mode[0] != 'f' => 9,
        //        10 when mode[0] != 'f' => 12,
        //        _ => "Unknown"
        //    };
        //}
        private string[] _ListTest = new string[0];

        public string[] ListTest
        {
            get => _ListTest ?? new string[0];
            set
            {
                _ListTest = value;
                OnPropertyChanged(nameof(ListTest));
            }
        }

        public IList GameModeSelectedItems => GameModesList.SelectedItems;
        private void GameModesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = GameModesList.SelectedItems;
            var len = selected.Count;
            string[] selectedGroups = new string[len];
            for (int i = 0; i < len; i++)
            {
                selectedGroups[i] = ((CollectionViewGroup)selected[i]).Name.ToString();
            }

            ListTest = selectedGroups;
        }
    }
}
