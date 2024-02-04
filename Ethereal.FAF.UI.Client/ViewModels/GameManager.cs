using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class GameManager : ObservableObject
    {
        [ObservableProperty]
        private ICommand _JoinGameCommand;
    }
}
