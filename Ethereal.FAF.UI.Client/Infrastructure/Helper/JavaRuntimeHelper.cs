using System.Text.RegularExpressions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Helper
{
    internal static class JavaRuntimeHelper
    {
        public static Regex ExceptionStackTraceRegex = new("(?:\\s+at ([^(]+)\\(([^:]+):(\\d+))\\)");
    }
}
