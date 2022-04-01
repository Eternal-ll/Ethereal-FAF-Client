namespace beta.Models.Server
{
    public class WelcomeData : Base.ServerMessage
    {
        public PlayerInfoMessage me { get; set; }
        public int id { get; set; }
        public string login { get; set; }
    }
}
