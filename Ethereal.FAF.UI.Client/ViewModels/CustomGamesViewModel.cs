using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class CustomGamesViewModel : Base.ViewModel
    {
        private readonly CollectionViewSource _gamesSource;
        private readonly LobbyGamesViewModel _lobbyGamesViewModel;
        private readonly IGameLauncher _gameLauncher;
        private readonly IContentDialogService _contentDialogService;

        private string _AllFeaturedModsLabel = "All featured mods";
        public CustomGamesViewModel(LobbyGamesViewModel lobbyGamesViewModel, IGameLauncher gameLauncher, IContentDialogService contentDialogService)
        {
            _lobbyGamesViewModel = lobbyGamesViewModel;
            _gamesSource = new();

            _SelectedFeaturedMod = _AllFeaturedModsLabel;
            _gameLauncher = gameLauncher;
            _contentDialogService = contentDialogService;
        }

        public ICollectionView GamesView => _gamesSource.View;

        private void RefreshViews()
        {
            GamesView.Refresh();
        }

        [ObservableProperty]
        private ObservableCollection<string> _FeaturedMods = new()
        {
            "All featured mods",
            FeaturedMod.FAF.ToString(),
            FeaturedMod.FAFDevelop.ToString(),
            FeaturedMod.FAFBeta.ToString(),
            FeaturedMod.Nomads.ToString()
        };

        #region SelectedFeaturedMod
        private string _SelectedFeaturedMod;
        public string SelectedFeaturedMod
        {
            get => _SelectedFeaturedMod;
            set
            {
                if (Set(ref _SelectedFeaturedMod, value)) RefreshViews();
            }
        }
        #endregion

        [ObservableProperty]
        private ObservableCollection<string> _GameStates = new()
        {
            GameState.Open.ToString(),
            GameState.Playing.ToString()
        };
        #region 
        private string _SelectedGameState = GameState.Open.ToString();
        public string SelectedGameState
        {
            get => _SelectedGameState;
            set
            {
                if (Set(ref _SelectedGameState, value)) RefreshViews();
                }
            }
        #endregion

        public override void OnLoaded()
        {
            base.OnLoaded();
            _gamesSource.Filter += ExtendedGamesViewFilter;
            _gamesSource.Source = _lobbyGamesViewModel.Games.AsObservable;
            OnPropertyChanged(nameof(GamesView));
        }

        public override void OnUnloaded()
        {
            _gamesSource.Filter -= ExtendedGamesViewFilter;
            _gamesSource.Source = null;
        }

        private void BasicGamesViewFilter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;
            if (e.Item is not Game game) return;
            if (game.GameType != GameType.Custom) return;

            //if (IsLive && !game.LaunchedAt.HasValue) return;
            //else if (!IsLive && game.LaunchedAt.HasValue) return;

            if (SelectedFeaturedMod != null
                && SelectedFeaturedMod != _AllFeaturedModsLabel
                && game.FeaturedMod.ToString() != SelectedFeaturedMod) return;

            e.Accepted = true;
        }
        private void ExtendedGamesViewFilter(object sender, FilterEventArgs e)
        {
            BasicGamesViewFilter(sender, e);
            if (!e.Accepted) return;
            var game = (Game)e.Item;
            e.Accepted = false;

            if (!SelectedGameState.Equals(game.State.ToString(), StringComparison.OrdinalIgnoreCase)) return;
            
            e.Accepted = true;
        }
        [RelayCommand]
        private async Task JoinGame(Game game)
        {
            string password = null;
            if (game.PasswordProtected)
            {
                var textbox = new TextBox(); 
                var result = await _contentDialogService.ShowSimpleDialogAsync(new()
                {
                    Title = "Enter secured password for lobby",
                    Content = textbox,
                    PrimaryButtonText = "Join",
                    SecondaryButtonText = string.Empty,
                    CloseButtonText = "Cancel",
                });
                if (result != Wpf.Ui.Controls.ContentDialogResult.Primary)
                {
                    return;
                }
                password = textbox.Text;
            }
            await _gameLauncher.JoinGameAsync(game, password);
        }

        [ObservableProperty]
        private bool _ListViewEnabled = true;
        [RelayCommand]
        private void ShowListView()
        {
            GridViewEnabled = false;
            ListViewEnabled = true;
        }
        [ObservableProperty]
        private bool _GridViewEnabled;
        [RelayCommand]
        private void ShowGridView()
        {
            ListViewEnabled = false;
            GridViewEnabled = true;
        }
    }
}