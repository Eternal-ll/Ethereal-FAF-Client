namespace FAF.Domain.LobbyServer.Enums
{
    public enum RatingType : int
    {
        global,
        ladder_1v1 = 2,
        tmm_2v2 = 3,
        tmm_4v4_full_share = 4,
        tmm_4v4_share_until_death = 5,
    }
}
