namespace FAF.Domain.LobbyServer
{
    public class MatchCancelled : Base.ServerMessage
    {
        public int game_id { get; set; }
        public string queue_name { get; set; }
    }
}
