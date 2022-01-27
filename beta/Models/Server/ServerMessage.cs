using beta.Models.Server.Base;
using System.Collections.Generic;

namespace beta.Models.Server
{
    public class ServerMessage : IServerMessage
    {
        public string command { get; set; }

        #region notice
        public string style { get; set; }
        public string text { get; set; }
        #endregion

        #region session
        public long session { get; set; }
        #endregion

        #region irc_password
        public string password { get; set; }
        #endregion

        #region welcome
        public PlayerInfoMessage me { get; set; }
        public int id { get; set; }
        public string login { get; set; }
        //public string country { get; set; }
        //public string clan { get; set; }
        #endregion

        #region social
        public string[] autojoin { get; set; }
        public string[] channels { get; set; }
        public long[] friends { get; set; }
        public long[] foes { get; set; }
        public int power { get; set; }
        #endregion

        #region player_info
        public string country { get; set; }
        public string clan { get; set; }
        public Dictionary<string, Rating> ratings { get; set; }
        public double[] global_rating { get; set; }
        public double[] ladder_rating { get; set; }
        public int number_of_games { get; set; }
        #endregion

        #region player_info //players
        public PlayerInfoMessage[] players { get; set; }
        #endregion

        #region game_info
        public string visibility { get; set; }
        public bool password_protected { get; set; }
        public int uid { get; set; }

        public string title { get; set; }
        public string state { get; set; }
        public string game_type { get; set; }
        public string featured_mod { get; set; }
        public Dictionary<string, string> sim_mods { get; set; }
        public string map_name { get; set; }
        public string map_file_path { get; set; }
        public string host { get; set; }
        public int num_players { get; set; }
        public int max_players { get; set; }
        public double? launched_at { get; set; }
        public string rating_type { get; set; }

        public double? rating_min { get; set; }

        public double? rating_max { get; set; }
        public bool enforce_rating_range { get; set; }
        public Dictionary<int, string[]> teams { get; set; }

        #endregion

        #region game_info //games
        public GameInfoMessage[] games { get; set; }
        #endregion

        #region matchmaker_info
        public string queue_name { get; set; }
        public string queue_pop_time { get; set; }
        public double queue_pop_time_delta { get; set; }
        //public int num_players { get; set; }
        public int[][] boundary_80s { get; set; }
        public int[][] boundary_75s { get; set; }
        #endregion

        #region matchmaker_info //queues
        public QueueMessage[] queues { get; set; }
        #endregion
    }
}
