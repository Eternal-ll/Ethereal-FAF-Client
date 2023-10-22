namespace FAF.Domain.LobbyServer
{
	public class AuthentificationFailedData : Base.ServerMessage
    {
        public string text { get; set; }

        //{
        //    "command":"authentication_failed",
        //    "text":"Token signature was invalid"
        //}
    }
}
