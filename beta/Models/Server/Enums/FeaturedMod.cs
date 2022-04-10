using System.Runtime.Serialization;

namespace beta.Models.Server.Enums
{
    /// <summary>
    /// featured_mod
    /// </summary>
    public enum FeaturedMod : byte
    {
        [EnumMember(Value = "FAF")]
        FAF = 0,
        [EnumMember(Value = "Ladder 1v1")]
        Ladder1v1 = 6,
        [EnumMember(Value = "FAF Beta")]
        FAFBeta = 27,
        [EnumMember(Value = "FAF Develop")]
        FAFDevelop = 28,

        //[EnumMember(Value = "Nomads")]
        Nomads = 4,

        // Depreciated
        murderparty = 1,

        labwars = 5,
        xtremewars = 12,
        diamond = 14,
        phantomx = 16,
        vanilla = 18,
        koth = 20,
        claustrophobia = 21,
        gw = 24,
        coop = 25,
        equilibrium = 29,
        tutorials = 30,
    }
}
