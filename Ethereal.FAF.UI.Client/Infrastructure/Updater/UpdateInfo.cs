using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Updater
{
    public class UpdateInfo
    {
        public Uri Url { get; set; } = null!;
        public string Version { get; set; } = null!;
        public DateTimeOffset ReleaseDate { get; set; }
        public bool IsMandatory { get; set; }
        public bool TryGetVersion(out Version version) => System.Version.TryParse(Version, out version);
    }
}
