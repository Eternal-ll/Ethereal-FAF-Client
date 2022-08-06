namespace FAF.Domain.LobbyServer
{
    /// <summary>
    /// Used by PlayerInfoMessage. 
    /// </summary>
    public class Rating
    {
        public double[] rating { get; set; }
        public int number_of_games { get; set; }
        public string name { get; set; }
    }
}
