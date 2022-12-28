namespace Ethereal.FAF.UI.Client.Models.IRC.Enums
{
    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    internal enum IrcUserFlags : byte
    {
        NORMAL = 0,
        WALLOPS = 1 << 1,
        INVISIBLE = 1 << 2
    }
}
