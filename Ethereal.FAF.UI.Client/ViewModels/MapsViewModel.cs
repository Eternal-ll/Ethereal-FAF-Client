using Ethereal.FAF.API.Client;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class MapsViewModel : Base.ViewModel
    {
        private readonly IFafApiClient FafApiClient;

        public MapsViewModel(IFafApiClient fafApiClient)
        {
            FafApiClient = fafApiClient;
        }
    }
}
