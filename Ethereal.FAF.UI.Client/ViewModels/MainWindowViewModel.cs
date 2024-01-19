using Ethereal.FAF.UI.Client.ViewModels.Dialogs;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class MainWindowViewModel : Base.ViewModel
    {
        public UpdateViewModel UpdateViewModel { get; init; }

        public MainWindowViewModel(UpdateViewModel updateViewModel)
        {
            UpdateViewModel = updateViewModel;
        }
    }
}