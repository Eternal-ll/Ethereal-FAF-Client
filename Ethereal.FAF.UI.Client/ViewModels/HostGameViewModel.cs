using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class GameHostingModel
    {
        public FeaturedMod FeaturedMod { get; set; }

        public string Title { get; set; }
        public string Password { get; set; }
        public string Visibility { get; set; }
        public int MinimumRating { get; set; }
        public int MaximumRating { get; set; }
        public bool EnforceRating { get; set; }
        public bool IsFriendsOnly { get; set; }
        public string Map { get; set; }
    }
    public sealed class HostGameViewModel : Base.ViewModel
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly ContainerViewModel Container;

        public HostGameViewModel(IServiceProvider serviceProvider, ContainerViewModel container)
        {
            ServiceProvider = serviceProvider;
            Game = new()
            {
                Title = "Ethereal FAF Client 2.0 [Test]",
            };
            Container = container;
        }
        public FeaturedMod[] FeaturedModSource => new FeaturedMod[]
        {
            FeaturedMod.FAF,
            FeaturedMod.FAFBeta,
            FeaturedMod.FAFDevelop,
            FeaturedMod.coop
        };

        #region Game
        private GameHostingModel _Game;
        public GameHostingModel Game
        {
            get => _Game;
            set => Set(ref _Game, value);
        }
        #endregion

        #region SelectedView
        private string _SelectedView;
        public string SelectedView
        {
            get=> _SelectedView;
            set
            {
                if (Set(ref _SelectedView, value))
                {
                    SelectionView = value switch
                    {
                        //"Local" => ServiceProvider.GetService<SelectLocalMapView>(),
                        //"API" => null,
                        //"Generator" => ServiceProvider.GetService<GenerateMapView>(),
                        _ => null
                    };
                    if (SelectionView is IGameHosting hosting)
                    {
                        hosting.SetHostingModel(Game);
                    }
                }
            }
        }
        #endregion

        #region SelectionView
        private UserControl _SelectionView;
        public UserControl SelectionView
        {
            get => _SelectionView;
            set => Set(ref _SelectionView, value);
        }

        #endregion

        #region LocalMapsCommand
        private ICommand _SelectViewCommand;
        public ICommand SelectViewCommand => _SelectViewCommand ??= new LambdaCommand(OnSelectViewCommand);

        private void OnSelectViewCommand(object obj)
        {
            SelectedView = null;
            SelectedView = obj?.ToString();
        }
        #endregion

        #region BackCommand
        private ICommand _BackCommand;
        public ICommand BackCommand => _BackCommand ??= new LambdaCommand(OnBackCommand);

        private void OnBackCommand(object obj)
        {
            if (SelectionView is null) return;
            ((IDisposable)SelectionView).Dispose();
        }
        #endregion
    }
}
