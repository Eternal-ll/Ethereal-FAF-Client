namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ContainerViewModel : Base.ViewModel
    {
        public BackgroundViewModel BackgroundViewModel { get; }

        public ContainerViewModel(BackgroundViewModel backgroundViewModel)
        {
            BackgroundViewModel = backgroundViewModel;
        }
    }
}
