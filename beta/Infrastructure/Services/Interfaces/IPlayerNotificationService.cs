using beta.Models.Server;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public enum PlayerState : byte
    {
        Connected,
        Disconnected,
        JoinedGame,
        LeftGame,
        FinishedGame
    }
    internal interface IPlayerNotificationService
    {

    }
}
