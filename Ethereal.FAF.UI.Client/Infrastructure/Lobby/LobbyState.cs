namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// 
    /// </summary>
    public enum LobbyState : int
    {
        None,
        Connecting,
        Connected,
        Authorizing,
        Authorized,
        Disconnecting,
        Disconnected
    }
}
