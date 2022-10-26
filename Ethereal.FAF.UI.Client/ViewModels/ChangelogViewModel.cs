using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ChangelogViewModel : Base.ViewModel
    {
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly IConfiguration Configuration;
        private readonly NotificationService NotificationService;

        public ChangelogViewModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, NotificationService notificationService)
        {
            HttpClientFactory = httpClientFactory;
            Configuration = configuration;
            NotificationService = notificationService;

            LoadChangelog();
        }

        private void LoadChangelog() => Task.Run(async () =>
        {
            var changelogUrl = Configuration.GetChangelogUrl();
            using var client = HttpClientFactory.CreateClient();
            var changelog = await client.GetStringAsync(changelogUrl)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var error = $"Failed to get changelog from {changelogUrl}";
                    NotificationService.Notify("Exception", error);
                    error += $"\nException:\n{t.Exception}";
                    return error;
                }
                NotificationService.Notify("Changelog", "Changelog loaded");
                return t.Result;
            });
            Changelog = changelog;
        });

        #region Changelog
        private string _Changelog;
        public string Changelog { get => _Changelog; set => Set(ref _Changelog, value); }
        #endregion
    }
}
