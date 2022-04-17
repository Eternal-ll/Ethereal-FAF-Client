using System.ComponentModel.DataAnnotations;

namespace beta.Models.Server.Enums
{
    public enum RatingType : int
    {
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Global")]
        global = 1,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "1 vs 1")]
        ladder_1v1 = 2,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "2 vs 2")]
        tmm_2v2 = 3,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "4 vs 4 FS")]
        tmm_4v4_full_share = 4,
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "4 vs 4 SUD")]
        tmm_4v4_share_until_death = 5,
    }
}
