namespace beta.Models.Enums
{
    /// <summary>
    /// States of <see cref="Infrastructure.Services.IrcService"/>
    /// </summary>
    public enum IrcState : byte
    {
        /// <summary>
        /// TCP client is disconnected
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// TCP client is connected
        /// </summary>
        Connected = 1,

        /// <summary>
        /// Pending TCP connection
        /// </summary>
        PendingConnection = 2,
        /// <summary>
        /// Cant connect to IRC server
        /// </summary>
        CantConnect = 3,

        /// <summary>
        /// Timed out on connection to IRC server
        /// </summary>
        TimedOut = 4,
        
        /// <summary>
        /// Pending IRC authorization
        /// </summary>
        PendingAuthorization = 5,
        /// <summary>
        /// Cant authorize in IRC
        /// </summary>
        CantAuthorize = 6,

        /// <summary>
        /// Authorized in IRC
        /// </summary>
        Authorized = 7,

        /// <summary>
        /// Too much reconnects to IRC server
        /// </summary>
        Throttled = 8,
    }
}
