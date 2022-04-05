using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace beta.ViewModels
{
    public class NavigationViewModel : ViewModel
    {
        private readonly IPlayersService PlayersService;
        private readonly IIrcService IrcService;
        private readonly IDownloadService DownloadService;
        public NavigationViewModel()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
            IrcService = App.Services.GetService<IIrcService>();
            DownloadService = App.Services.GetService<IDownloadService>();

            PlayersService.MeReceived += PlayersService_MeReceived;
            IrcService.StateChanged += IrcService_StateChanged;
            DownloadService.NewDownload += DownloadService_NewDownload;
            DownloadService.DownloadEnded += DownloadService_DownloadEnded;
        }

        private void DownloadService_DownloadEnded(object sender, DownloadViewModel e)
        {
            if (e.Equals(LatestDownloadViewModel)) LatestDownloadViewModel = null;
        }

        private void DownloadService_NewDownload(object sender, DownloadViewModel e) => LatestDownloadViewModel = e;

        private void IrcService_StateChanged(object sender, IrcState e) => IrcState = e;

        private void PlayersService_MeReceived(object sender, PlayerInfoMessage e) => Me = e;

        #region Me
        private PlayerInfoMessage _Me;
        private PlayerInfoMessage Me
        {
            get => _Me;
            set => Set(ref _Me, value);
        }

        #endregion

        #region IrcState
        private IrcState _IrcState;
        public IrcState IrcState
        {
            get => _IrcState;
            set
            {
                if (!Equals(value, _IrcState))
                {
                    _IrcState = value;
                    OnPropertyChanged(nameof(IrcState));
                }
            }
        }
        #endregion

        #region LatestDownloadViewModel
        private DownloadViewModel _LatestDownloadViewModel;
        public DownloadViewModel LatestDownloadViewModel
        {
            get => _LatestDownloadViewModel;
            set => Set(ref _LatestDownloadViewModel, value);
        } 
        #endregion
    }
}
