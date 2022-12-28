using System.Runtime.Serialization;

namespace FAF.Domain.LobbyServer.Enums
{
    public enum GameType : byte
    {
        Custom = 0,
        Coop = 1,

        [EnumMember(Value = "TMM")]
        MatchMaker = 2,
    }
}
