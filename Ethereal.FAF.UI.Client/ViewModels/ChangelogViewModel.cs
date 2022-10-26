using System.Net.Http;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ChangelogViewModel : Base.ViewModel
    {
        private readonly IHttpClientFactory HttpClientFactory;

        public ChangelogViewModel(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }
    }
}
