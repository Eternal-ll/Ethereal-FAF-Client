using System.Text.RegularExpressions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    public readonly record struct GameMapInfo(string Raw, string Name, string Version);
    internal static class GameMapHelper
    {
        public static Regex CommonNamePattern = new("(.*).v(\\d*)");

        public static GameMapInfo Parse(string name)
        {
            var result = CommonNamePattern.Match(name);
            if (result.Groups.Count == 3)
            {
                return new(name, result.Groups[1].Value, result.Groups[3].Value);
            }
            return new(name, name, null);
        }
    }
}
