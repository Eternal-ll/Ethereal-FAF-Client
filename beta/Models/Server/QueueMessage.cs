using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public class QueueMessage : IServerMessage
    {
        public string command { get; set; }
        public QueueMessage[] queues { get; set; }

        public string queue_name { get; set; }
        public string queue_pop_time { get; set; }
        public double queue_pop_time_delta { get; set; }
        public int num_players { get; set; }
        public int[][] boundary_80s { get; set; }
        public int[][] boundary_75s { get; set; }
    }
}
