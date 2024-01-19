namespace FAF.Domain.LobbyServer.Outgoing
{
    public class PingCommand : OutgoingCommand
    {
        public PingCommand() : base("ping")
        {
        }
    }
}
