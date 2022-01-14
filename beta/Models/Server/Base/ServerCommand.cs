namespace beta.Models.Server.Base
{
    public enum ServerCommand
    {
        unknown = -10,

        ping = -1,
        pong = 2,

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
        #endregion

        // WRITE
        set_party_factions = 20,
        restore_game_session = 21,
        game_matchmaking = 22,

        game_host = 23,
        game_join = 24,

        invite_to_party = 25,



    }
}
