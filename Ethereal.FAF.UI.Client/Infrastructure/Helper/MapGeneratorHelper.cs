using System.Text.RegularExpressions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    internal static class MapGeneratorHelper
    {
        public static Regex Pattern = new Regex(@"neroxis_map_generator_(\d+\.\d+\.\d+)_(.*)");
        public static bool IsGeneratedMap(string mapName) => Pattern.IsMatch(mapName);
    }
}
