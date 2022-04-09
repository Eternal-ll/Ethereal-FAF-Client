namespace beta.Models.API.Enums
{
    public enum ApiDataType : byte
    {
        mod,
        modVersion,
        modReviewsSummary,

        map,
        mapVersion,
        mapReviewsSummary,
        mapStatistics,

        featuredModFile,

        player,
        clanMembership,
        avatar,
        avatarAssignment,
        nameRecord,
        globalRating,
        ladder1v1Rating
    }
}
