using beta.Models.Server;
using beta.ViewModels.Base;

namespace beta.ViewModels
{
    public interface ISelectedPlayerProfile
    {

    }
    public class HomeViewModel : ViewModel
    {       
        public HomeViewModel()
        {
            NewsViewModel = new();
            ForumViewModel = new();
            PlayersViewModel = new();
            CustomGamesViewModel = new();

            PlayersViewModel.PropertyChanged += PlayersViewModel_PropertyChanged;
        }

        private void PlayersViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PlayersViewModel.SelectedPlayer)) return;

            var selectedPlayer = PlayersViewModel.SelectedPlayer;

            if (selectedPlayer is not null && selectedPlayer is PlayerInfoMessage player)
            {
                SelectedPlayerProfile = new ProfileViewModel(player);
            }
            else
            {
                SelectedPlayerProfile = new PlugViewModel();
            }
        }

        public NewsViewModel NewsViewModel { get; set; }
        public ForumViewModel ForumViewModel { get; set; }
        public PlayersViewModel PlayersViewModel { get; set; }
        public CustomGamesViewModel CustomGamesViewModel { get; set; }

        #region SelectedPlayerProfile
        private ISelectedPlayerProfile _SelectedPlayerProfile = new PlugViewModel();
        public ISelectedPlayerProfile SelectedPlayerProfile
        {
            get => _SelectedPlayerProfile;
            set
            {
                if (Set(ref _SelectedPlayerProfile, value))
                {

                }
            }
        }
        #endregion
    }
}
