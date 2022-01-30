using beta.Models.Server.Base;
using System;
using System.Collections.Generic;

namespace beta.Models.Server
{
    public class PlayerInfoMessage : IServerMessage
    {
        #region Custom properties

        #region Flag
        private object _Flag;
        public object Flag => _Flag ??= App.Current.Resources["Flag." + country];
        #endregion

        public DateTime? Updated { get; set; }

        #endregion
        public int id { get; set; }
        public string login { get; set; }
        public string country { get; set; }
        public string clan { get; set; }
        public Dictionary<string, Rating> ratings { get; set; }
        public double[] global_rating { get; set; }
        public double[] ladder_rating { get; set; }
        public int number_of_games { get; set; }
        public string command { get; set; }

        public PlayerInfoMessage[] players { get; set; }
    }
}
