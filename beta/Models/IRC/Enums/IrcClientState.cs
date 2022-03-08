namespace beta.Models.IRC.Enums
{
    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    internal enum IrcClientState : byte
    {
        PENDING_SERVER_CONNECTION,
        CONNECTED_TO_SERVER,
        PENDING_CHANNEL_CONNECTION,
        CONNECTED_TO_CHANNEL
    }
}
