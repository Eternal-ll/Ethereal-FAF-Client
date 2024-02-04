using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using System.Windows;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for GamesView.xaml
    /// </summary>
    public sealed partial class GamesView : INavigableView<CustomGamesViewModel>
    {
        public GamesView(CustomGamesViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            //Resources.Add("WatchGameCommand", viewModel.WatchGameCommand);
            //Resources.Add("RemoveMapFromBlacklistCommand", viewModel.RemoveMapFromBlacklistCommand);
            //Resources.Add("AddMapToBlacklistCommand", viewModel.AddMapToBlacklistCommand);
        }

        public CustomGamesViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
