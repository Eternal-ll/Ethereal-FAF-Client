namespace FAF.Domain.LobbyServer.Outgoing
{
    public class PongCommand : OutgoingCommand
    {
        public PongCommand() : base("pong")
        {
        }
    }
}
