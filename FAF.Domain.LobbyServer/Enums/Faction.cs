using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Enums
{
    /// <summary>
    /// Enum to represent factions. Numbers match up with faction identification
    /// ids from the game.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Faction : int
    {
        UEF = 1,
        AEON = 2,
        CYBRAN = 3,
        SERAPHIM = 4,

        RANDOM = 5,
    }
}
