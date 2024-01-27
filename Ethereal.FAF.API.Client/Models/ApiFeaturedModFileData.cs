namespace Ethereal.FAF.API.Client.Models
{
    public class FeaturedModFile : Base.ApiUniversalData
    {
        public string Url => Attributes?["url"];
        public string Name => Attributes?["name"];
        public string Group => Attributes?["group"];
        public int Version => int.TryParse(Attributes?["version"], out var version) ? version : -1;
        public string MD5 => Attributes?["md5"];

        public string CacheableUrl => Attributes?["cacheableUrl"];
        public string HmacToken => Attributes?["hmacToken"];
    }
}
