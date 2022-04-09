using beta.Infrastructure.Services.Interfaces;
using beta.Models.Enums;
using beta.Models.Server;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace beta.ViewModels
{
    public class NavigationViewModel : ViewModel
    {
        public event EventHandler LogoutRequested;

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

            Login = Properties.Settings.Default.PlayerNick;
        }

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

        #region Login
        private string _Login;
        public string Login
        {
            get => _Login;
            set => Set(ref _Login, value);
        }
        #endregion

        #region Views
        private readonly Dictionary<Type, ViewModel> ViewModels = new();
        #endregion

        #region CurrentViewTag
        private string _CurrentViewTag;
        public string CurrentViewTag
        {
            get => _CurrentViewTag;
            set
            {
                if (value is null) return;
                if (Set(ref _CurrentViewTag, value))
                {
                    if (value == "Logout")
                    {
                        LogoutRequested?.Invoke(this, null);
                        // TODO
                        return;
                    }

                    string pageName = "beta.ViewModels." + value + "ViewModel";
                    Type viewType = typeof(ViewModel).Assembly.GetType(pageName);
                    ViewModel viewModel = null;
                    if (ViewModels.TryGetValue(viewType, out var cachedViewModel))
                    {
                        if (cachedViewModel is null)
                        {
                            object[] args = null;
                            if (viewType == typeof(ProfileViewModel))
                            {
                                args = new object[] { Me.id };
                            }
                            viewModel = (ViewModel)Activator.CreateInstance(viewType, args);
                            ViewModels[viewType] = viewModel;
                        }
                        else
                        {
                            viewModel = cachedViewModel;
                        }
                    }
                    else
                    {
                        viewModel = (ViewModel)Activator.CreateInstance(viewType);
                    }
                    SelectedViewModel = viewModel;
                }
            }
        }
        #endregion

        #region SelectedViewModel
        private ViewModel _SelectedViewModel = new PlugViewModel();
        public ViewModel SelectedViewModel
        {
            get => _SelectedViewModel;
            set
            {
                if (Set(ref _SelectedViewModel, value))
                {

                }
            }
        }
        #endregion

        private void DownloadService_DownloadEnded(object sender, DownloadViewModel e)
        {
            if (e.Equals(LatestDownloadViewModel)) LatestDownloadViewModel = null;
        }

        private void DownloadService_NewDownload(object sender, DownloadViewModel e) => LatestDownloadViewModel = e;

        private void IrcService_StateChanged(object sender, IrcState e) => IrcState = e;

        private void PlayersService_MeReceived(object sender, PlayerInfoMessage e) => Me = e;
    }
}
