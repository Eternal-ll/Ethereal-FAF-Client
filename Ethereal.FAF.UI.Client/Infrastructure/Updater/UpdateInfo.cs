using Ethereal.FAF.UI.Client.Models.Update;
using System;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Updater
{
    public class UpdateInfo
    {
        public Uri Url { get; set; } = null!;
        public string Version { get; set; } = null!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UpdateChannel Channel { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UpdateType Type { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public bool TryGetVersion(out Version version) => System.Version.TryParse(Version, out version);
    }
}
