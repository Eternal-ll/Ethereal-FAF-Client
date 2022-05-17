using AsyncAwaitBestPractices;
using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.API;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace beta.ViewModels
{
    internal class HostGameViewModel : Base.ViewModel
    {
        public event EventHandler Finished;
        public PlayerInfoMessage Me { get; private set; }

        private readonly IPlayersService PlayersService;
        private readonly IGameSessionService GameSessionService;
        private readonly INotificationService NotificationService;

        public HostGameViewModel()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
            GameSessionService = App.Services.GetService<IGameSessionService>();
            NotificationService = App.Services.GetService<INotificationService>();

            Me = PlayersService.Self;
            Maps = new();
            Maps.MapSelected += (s, e) => SelectedMap = e;
        }

        public MapsView Maps { get; set; }

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
                Set(ref _SelectedMapName, value);
            }
        }
        #endregion

        #region SelectedMap
        private ApiMapData _SelectedMap;
        public ApiMapData SelectedMap
        {
            get => _SelectedMap;
            set => Set(ref _SelectedMap, value);
        }
        #endregion

        #region HostGameCommand
        private ICommand _HostGameCommand;
        public ICommand HostGameCommand => _HostGameCommand ??= new LambdaCommand(OnHostGameCommand, CanHostGameCommand);
        private bool CanHostGameCommand(object parameter) => SelectedMap is not null;
        private void OnHostGameCommand(object parameter)
        {
            if (SelectedMap is null)
            {
                NotificationService.ShowPopupAsync("Map is not selected");
                return;
            }
            Finished?.Invoke(this, null);

            string title = string.IsNullOrWhiteSpace(Title) ? "Ethereal lobby" : Title;
            GameSessionService.HostGame(title, FeaturedMod, SelectedMap.FolderName, MinAllowedRating, MaxAllowedRating,
                IsFriendsOnly ? GameVisibility.Friends : GameVisibility.Public, IsRatingRestrictionEnabled, Password)
                .SafeFireAndForget(onException: ex => NotificationService.ShowExceptionAsync(ex));
        }
        #endregion
    }
}
