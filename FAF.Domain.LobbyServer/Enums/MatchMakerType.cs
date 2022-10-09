namespace FAF.Domain.LobbyServer.Enums
{
    /// <summary>
    /// https://api.faforever.com/data/matchmakerQueue
    /// </summary>
    public enum MatchmakingType : int
    {
        ladder1v1 = 1,
        tmm2v2 = 2,
        tmm4v4_full_share = 3,
        tmm4v4_share_until_death = 4,
    }
}
