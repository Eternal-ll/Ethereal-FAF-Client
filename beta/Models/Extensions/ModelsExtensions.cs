using beta.Models.Server.Enums;

namespace beta.Models.Extensions
{
    public static class ModelsExtensions
    {
        public static string FormatToString(this RatingType ratingType) => ratingType switch
        {
            RatingType.global => "Global",
            RatingType.ladder_1v1 => "Ladder 1 vs 1",
            RatingType.tmm_2v2 => "TMM 2 vs 2",
            RatingType.tmm_4v4_full_share => "TMM 4 vs 4 FS",
            RatingType.tmm_4v4_share_until_death => "TMM 4 vs 4 SUD",
            _ => ratingType.ToString(),
        };
    }
}
