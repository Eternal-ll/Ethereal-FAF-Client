using beta.Models.API.Enums;
using System.Collections.Generic;

namespace beta.Models.API.Base
{
    public static class ApiUniversalTools
    {
        public static Dictionary<string, string> GetAttributesFromIncluded(ApiUniversalWithAttributes[] included, ApiDataType type, int id)
        {
            if (included is null) return null;

            for (int i = 0; i < included.Length; i++)
            {
                var item = included[i];

                if (item.Type != type) continue;

                if (item.Id == id)
                    return item.Attributes;
            }

            return null;
        }
    }
}
