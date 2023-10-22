using System.Drawing;

namespace Ethereal.FAF.UI.Client.Infrastructure.Extensions
{
    internal static class ColorExtensions
    {
        public static string ToHexString(this Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }
}
