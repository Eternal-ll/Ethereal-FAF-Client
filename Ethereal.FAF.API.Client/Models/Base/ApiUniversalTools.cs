using Ethereal.FAF.API.Client.Models.Enums;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public static class ApiUniversalTools
    {
        public static ApiUniversalData GetDataFromIncluded(ApiUniversalData[] included, ApiDataType type, int? id)
        {
            if (included is null) return null;

            for (int i = 0; i < included.Length; i++)
            {
                var item = included[i];

                if (item.Type != type) continue;

                if (id.HasValue && item.Id != id) continue;

                return item;
            }

            return null;
        }
    }
}
