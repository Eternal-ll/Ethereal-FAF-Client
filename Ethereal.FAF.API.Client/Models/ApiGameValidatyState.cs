﻿namespace Ethereal.FAF.API.Client.Models
{
    /// <summary>
    /// Game validaty state 
    /// </summary>
    public enum ApiGameValidatyState : int
    {
        UNKNOWN = -1,
        VALID,
        TOO_MANY_DESYNCS,
        WRONG_VICTORY_CONDITION,
        NO_FOG_OF_WAR,
        CHEATS_ENABLED,
        PREBUILT_ENABLED,
        NORUSH_ENABLED,
        BAD_UNIT_RESTRICTIONS,
        BAD_MAP,
        TOO_SHORT,
        BAD_MOD,
        COOP_NOT_RANKED,
        MUTUAL_DRAW,
        SINGLE_PLAYER,
        FFA_NOT_RANKED,
        UNEVEN_TEAMS_NOT_RANKED,
        UNKNOWN_RESULT,
        TEAMS_UNLOCKED,
        MULTIPLE_TEAMS,
        HAS_AI,
        CIVILIANS_REVEALED,
        WRONG_DIFFICULTY,
        EXPANSION_DISABLED,
        SPAWN_NOT_FIXED,
        OTHER_UNRANK,
        UNRANKED_BY_HOST
    }
}
