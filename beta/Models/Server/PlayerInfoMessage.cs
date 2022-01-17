using beta.Views;
using System.Collections.Generic;

namespace beta.Models.Server
{
    public class PlayerInfoMessage : MainView.IServerMessage
    {
        public int id { get; set; }
        public string login { get; set; }
        public string country { get; set; }
        public string clan { get; set; }
        public Dictionary<string, MainView.Rating> ratings { get; set; }
        public double[] global_rating { get; set; }
        public double[] ladder_rating { get; set; }
        public int number_of_games { get; set; }
        public string command { get; set; }


        public PlayerInfoMessage[] players { get; set; }
    }
}
