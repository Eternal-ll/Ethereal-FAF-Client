using System;

namespace beta.Models.API
{
    public class ApiFeaturedModFileData : Base.ApiUniversalWithAttributes
    {
        public Uri Url => Uri.TryCreate(Attributes?["url"], UriKind.Absolute, out var uri) ? uri : null;
        public string Name => Attributes?["name"];
        public string Group => Attributes?["group"];
        public int Version => int.TryParse(Attributes?["version"], out var version) ? version : -1;
        public string MD5 => Attributes?["md5"];
    }
}
