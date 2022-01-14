using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors.Core;
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

        #region LobbiesCountOnView
        public int LobbiesCountOnView
        {
            get
            {
                int sum = 0;
                //if(LobbiesRepeater.ItemsSourceView!=null)
                //    for (int i = 0; i < LobbiesRepeater.ItemsSourceView.Count; i++)
                //    {
                //        sum += ((CollectionViewGroup)LobbiesRepeater.ItemsSourceView.GetAt(i)).ItemCount;
                //    }
                return sum;
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
                //OnPropertyChanged(nameof(LobbiesCountOnView));
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
                //OnPropertyChanged(nameof(LobbiesCountOnView));
            }
        } 
        #endregion

        //private bool?[] SelectedGameModes = new bool?[15];
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

        private readonly IList<KeyValuePair<long, int>> NewLobbies = new List<KeyValuePair<long, int>>();
        private readonly IList<GameInfoMessage> RequestedLobbiesToRemove = new List<GameInfoMessage>();
        #region LobbiesViewSourceFilter
        private void LobbiesViewSourceFilter(object sender, FilterEventArgs e)
        {
            string searchText = _SearchText;
            var lobby = (GameInfoMessage)e.Item;

            //if (lobby.num_players == 0)
            //{
            //}
            //if (lobby.launched_at != null)
            //{
            //    e.Accepted = false;
            //    //Lobbies.Remove(lobby);
            //    return;
            //}
            
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
                //if (lobby.map_file_path.Contains("gap") || lobby.map_file_path.Contains("crater"))
                //    return;

                //bool globalFound = false;
                for (int i = 0; i < Lobbies.Count; i++)
                {
                    //bool found = false;
                    var cLobby = Lobbies[i];

                    //if(cLobby.num_players==0) cLobby.LifyCycle++;

                    if (cLobby.uid == lobby.uid)
                    {
                        //lobby.LifyCycle = cLobby.LifyCycle;
                        //found = true;
                        //globalFound = true;
                        Lobbies[i] = lobby;

                        if (lobby.launched_at != null) 
                            Lobbies.RemoveAt(i); 
                        return;
                    }
                    
                    //if (cLobby.LifyCycle == 50)
                    //{
                    //    Lobbies.RemoveAt(i);
                    //    continue;
                    //}
                    
                    //Lobbies[i] = found ? lobby : cLobby;
                }

                //if (!globalFound)
                //{
                if (lobby.launched_at == null)
                    Lobbies.Add(lobby);
                //}
                
                //CheckLobby(lobby);
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

                //bool found = false;
                //for (int i = 0; i < Lobbies.Count; i++)
                //{
                //    if (Lobbies[i].uid == lobby.uid)
                //    {
                //        Lobbies[i] = e.Arg;
                //        found = true;
                //        break;
                //    }
                //}


                //if (!found)
                Lobbies.Add(lobby);
                //else
                //    for (int i = 0; i < NewLobbies.Count; i++)
                //    {
                //        var nlobby = NewLobbies[i];

                //        if (lobby.uid == nlobby.Key)
                //            if (lobby.num_players > 0)
                //            {
                //                NewLobbies.RemoveAt(i);
                //                break;
                //            }
                //    }

                //CheckLobby(lobby);
            });
        }

        private void CheckLobby(GameInfoMessage lobby)
        {
            bool found = false;
            
            for (int i = 0; i < NewLobbies.Count; i++)
            {
                var nlobby = NewLobbies[i];

                if (lobby.uid == nlobby.Key)
                {
                    found = true;
                    if (lobby.num_players > 0)
                    {
                        NewLobbies.RemoveAt(i);
                        break;
                    }
                    
                    if (nlobby.Value < 10)
                        continue;
                    Lobbies.Remove(lobby);
                }

                NewLobbies[i] = new KeyValuePair<long, int>(nlobby.Key, nlobby.Value + 1);
            }

            if (!found)
            {
                NewLobbies.Add(new KeyValuePair<long, int>(lobby.uid, 0));
            }
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
            OnPropertyChanged(nameof(LobbiesCountOnView));
        }

        private ActionCommand _MoveToSearchBox;
        public ICommand MoveToSearchBox => _MoveToSearchBox ??= new ActionCommand(MoveViewToSearchBox);

        private void MoveViewToSearchBox()
        {
            //Frame.ScrollToHorizontalOffset(1000);
            //Frame.ScrollToVerticalOffset(1060);
            var target = Viewbox.ActualHeight + 90;
            var scrollViewer = Frame;

            var scrollTimer = new DispatcherTimer();
            var t = scrollViewer.HorizontalOffset;
            scrollTimer.Start();

            scrollTimer.Interval = TimeSpan.FromMilliseconds(10);

            scrollTimer.Tick += (s, e) =>
            {          
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 20);

                if (scrollViewer.VerticalOffset >= target)
                {
                    scrollTimer.Stop();
                }
            };
        }
    }
}
