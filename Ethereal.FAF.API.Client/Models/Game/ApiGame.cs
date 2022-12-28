namespace Ethereal.FAF.API.Client.Models.Game
{
    public class ApiGame : beta.Models.API.Base.ApiUniversalData
    {
        public string Name => Attributes["name"];
        public bool IsReplayAvailable => bool.Parse(Attributes["replayAvailable"]);
        public string replayTicks => Attributes["replayTicks"];
        public string ReplayUrl => Attributes["replayUrl"];
        public ApiGameValidatyState Validity => Enum.Parse<ApiGameValidatyState>(Attributes["validity"]);
        public string VictoryCondition => Attributes["victoryCondition"];
        public DateTime StartTime => DateTime.Parse(Attributes["startTime"]);
        public DateTime EndTime => DateTime.Parse(Attributes["endTime"]);
    }
}
