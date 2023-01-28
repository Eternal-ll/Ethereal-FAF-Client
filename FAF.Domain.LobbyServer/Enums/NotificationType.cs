using System;

namespace FAF.Domain.LobbyServer.Enums
{
    /// <summary>
    /// <see cref="Notification"/>
    /// </summary>
    public enum NotificationType : byte
    {
        Info,
        Error,
        Warning,
        Scores,
        [Obsolete]
        /// <summary>
        /// Someone killed your game
        /// </summary>
        Kill,
        /// <summary>
        /// Kick from lobby
        /// </summary>
        [Obsolete]
        Kick,
    }
}
