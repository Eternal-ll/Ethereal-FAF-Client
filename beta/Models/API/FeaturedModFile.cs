using System.Collections.Generic;

namespace beta.Models.API
{
    public class FeaturedModFile
    {
        public string FileSize { get; set; }
        public string type { get; set; }
        public string id { get; set; }
        public Dictionary<string, object> attributes { get; set; }
    }
}
