using System;

namespace beta.Models.Server
{
    public class MatchCancelledData : Base.ServerMessage
    {
        public int game_id { get; set; }
        public string queue_name { get; set; }
    }
    public class MatchFoundData : Base.ServerMessage
    {
        public string queue_name { get; set; }
        public int game_id { get; set; }
    }
    /// <summary>
    /// TODO
    /// <see cref="https://github.com/FAForever/server/issues/607"/>
    /// <seealso cref="https://github.com/FAForever/downlords-faf-client/issues/1783"/>
    /// </summary>
    public class MatchInfoData : Base.ServerMessage
    {
        public DateTime expires_at { get; set; }
        public int players_total { get; set; }
        public int players_ready { get; set; }
        public bool ready { get; set; }
    }
}
