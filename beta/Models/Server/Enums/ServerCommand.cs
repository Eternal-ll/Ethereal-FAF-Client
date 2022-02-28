namespace beta.Models.Server.Enums
{
    public enum ServerCommand : byte
    {
        #region Receive
        // READ

        notice = 0,

        session = 1,
        irc_password = 2,
        welcome = 3,
        social = 4,
        player_info = 5,
        game_info = 6,
        matchmaker_info = 7,
        mapvault_info = 8,

        ping = 9,
        pong = 10,
        #endregion

        // WRITE
        set_party_factions = 20,
        restore_game_session = 21,
        game_matchmaking = 22,

        game_host = 23,
        game_join = 24,

        invite_to_party = 25,

        // on lobby server authorization
        invalid = 99,

        unknown = 100
    }
}
