using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using System.Windows.Input;

namespace beta.ViewModels
{
    internal class HostGameViewModel : Base.ViewModel
    {
        public PlayerInfoMessage Me { get; private set; }

        private readonly IMapsService MapsService;
        private readonly IPlayersService PlayersService;
        private readonly IGameSessionService GameSessionService;

        public HostGameViewModel()
        {
            MapsService=App.Services.GetService<IMapsService>();
            PlayersService = App.Services.GetService<IPlayersService>();
            GameSessionService = App.Services.GetService<IGameSessionService>();

            Me = PlayersService.Me;

            LocalMapsName = MapsService.GetLocalMaps();
            MapsViewSource.Source = LocalMapsName;

            MapsViewSource.Filter += MapsViewSource_Filter;
        }

        private void MapsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MapFilterText)) return;
            e.Accepted = e.Item.ToString().Contains(MapFilterText, System.StringComparison.OrdinalIgnoreCase);
        }

        #region Title
        private string _Title;
        [Required(ErrorMessage = "Please enter title")]
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion

        #region Password
        private string _Password;
        public string Password
        {
            get => _Password;
            set => Set(ref _Password, value);
        }
        #endregion

        #region IsFriendsOnly
        private bool _IsFriendsOnly;
        public bool IsFriendsOnly
        {
            get => _IsFriendsOnly;
            set => Set(ref _IsFriendsOnly, value);
        }
        #endregion

        #region IsRatingRestrictionEnabled
        private bool _IsRatingRestrictionEnabled;
        public bool IsRatingRestrictionEnabled
        {
            get => _IsRatingRestrictionEnabled;
            set => Set(ref _IsRatingRestrictionEnabled, value);
        }
        #endregion

        private int? GetGlobal()
        {
            if (Me is null) return null;
            if (Me.ratings.TryGetValue("global", out var rating)) return rating.DisplayedRating;
            return null;
        }

        public int MinRatingRange => GetGlobal().HasValue ? GetGlobal().Value - 500 : -500;
        public int MaxRatingRange => GetGlobal().HasValue ? GetGlobal().Value + 300 : 3000;

        #region MinAllowedRating
        private int _MinAllowedRating;
        public int MinAllowedRating
        {
            get => _MinAllowedRating;
            set => Set(ref _MinAllowedRating, value);
        }
        #endregion

        #region MaxAllowedRating
        private int _MaxAllowedRating;
        public int MaxAllowedRating
        {
            get => _MaxAllowedRating;
            set => Set(ref _MaxAllowedRating, value);
        }
        #endregion

        public static FeaturedMod[] FeaturedMods => new[]
        {
            FeaturedMod.FAF,
            FeaturedMod.FAFBeta,
            FeaturedMod.FAFDevelop,
        };

        #region FeaturedMod
        private FeaturedMod _FeaturedMod;
        public FeaturedMod FeaturedMod
        {
            get => _FeaturedMod;
            set => Set(ref _FeaturedMod, value);
        }
        #endregion

        #region SelectedMapName
        private string _SelectedMapName;
        public string SelectedMapName
        {
            get => _SelectedMapName;
            set
            {
                if (Set(ref _SelectedMapName, value))
                {
                    if (value is null) SelectedGameMap = null;
                    else SelectedGameMap = MapsService.GetGameMap(value).Result;
                }
            }
        }
        #endregion

        #region SelectedGameMap
        private GameMap _SelectedGameMap;
        public GameMap SelectedGameMap
        {
            get => _SelectedGameMap;
            set => Set(ref _SelectedGameMap, value);
        }
        #endregion

        #region MapFilterText
        private string _MapFilterText;
        public string MapFilterText
        {
            get => _MapFilterText;
            set
            {
                if (Set(ref _MapFilterText, value))
                {
                    MapsView.Refresh();
                }
            }
        }
        #endregion

        public string[] LocalMapsName { get; private set; }
        private readonly CollectionViewSource MapsViewSource = new();
        public ICollectionView MapsView => MapsViewSource.View;

        #region HostGameCommand
        private ICommand _HostGameCommand;
        public ICommand HostGameCommand => _HostGameCommand ??= new LambdaCommand(OnHostGameCommand);
        private void OnHostGameCommand(object parameter)
        {
            string title = string.IsNullOrWhiteSpace(Title) ? "Ethereal lobby" : Title;
            GameSessionService.HostGame(title, FeaturedMod, SelectedMapName, MinAllowedRating, MaxAllowedRating,
                IsFriendsOnly ? GameVisibility.Friends : GameVisibility.Public, IsRatingRestrictionEnabled, Password);
        }
        #endregion
    }
}
