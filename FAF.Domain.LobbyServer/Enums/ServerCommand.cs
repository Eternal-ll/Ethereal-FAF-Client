namespace FAF.Domain.LobbyServer.Enums
{
    public enum ServerCommand : byte
    {
        /// <summary>
        /// Auth
        /// </summary>
        auth,
        /// <summary>
        /// Authentification
        /// </summary>
        ask_session,
        /// <summary>
        /// Authentification failse.
        /// </summary>
        authentication_failed,
        /// <summary>
        /// Incoming notification
        /// </summary>
        notice,
        /// <summary>
        /// Incoming session id
        /// </summary>
        session,
        /// <summary>
        /// Incoming IRC password
        /// </summary>
        irc_password,
        /// <summary>
        /// Incoming welcome message
        /// </summary>
        welcome,
        /// <summary>
        /// Incoming social data
        /// </summary>
        social,
        /// <summary>
        /// Incoming information about player(s)
        /// </summary>
        player_info,
        /// <summary>
        /// Incoming information about game(s)
        /// </summary>
        game_info,
        /// <summary>
        /// Incoming data for Ice-Adapter
        /// </summary>
        game,
        /// <summary>
        /// Incoming Match Maker info
        /// </summary>
        matchmaker_info,
        /// <summary>
        /// Incoming Map vault info
        /// </summary>
        mapvault_info,
        /// <summary>
        /// Incoming ping command
        /// </summary>
        ping,
        /// <summary>
        /// Outgoing pong command
        /// </summary>
        pong,
        /// <summary>
        /// Incoming game launch message
        /// </summary>
        game_launch,
        /// <summary>
        /// Incoming invite to party for Team Match Making
        /// </summary>
        party_invite,
        /// <summary>
        /// Incoming new information about party
        /// </summary>
        update_party,
        /// <summary>
        /// Outgoing invite to <recipient_id> to join the party
        /// </summary>
        invite_to_party,
        /// <summary>
        /// Incoming kick from party
        /// </summary>
        kicked_from_party,
        /// <summary>
        /// Outgoing command to set party factions
        /// </summary>
        set_party_factions,
        /// <summary>
        /// Match confirmation info https://github.com/FAForever/server/issues/607
        /// </summary>
        match_info,
        /// <summary>
        /// Match confirmation to start https://github.com/FAForever/server/issues/607
        /// </summary>
        match_ready,
        /// <summary>
        /// Incoming message that matchmaker match is found
        /// </summary>
        match_found,
        /// <summary>
        /// Incoming message that matchmaker match cancelled
        /// </summary>
        match_cancelled,
        /// <summary>
        /// Incoming TMM queue information?
        /// </summary>
        search_info,
        /// <summary>
        /// Outgoing command to restore game session on reconnect to lobby-server
        /// </summary>
        restore_game_session,
        /// <summary>
        /// Join/Leave from TMM queue
        /// </summary>
        game_matchmaking,
        /// <summary>
        /// Outgoing command to host game
        /// </summary>
        game_host,
        /// <summary>
        /// Outgoing command to join game with <uid>
        /// </summary>
        game_join,
        /// <summary>
        /// Incoming/Outgoing message for ICE servers
        /// </summary>
        ice_servers,
        /// <summary>
        /// Incoming message that we did something wrong and the lobby-server will disconnect
        /// </summary>
        invalid,


        // ICE commands ------------------

        // Using IceUniversalData as wrap class

        JoinGame,
        HostGame,
        ConnectToPeer,
        DisconnectFromPeer,
        IceMsg

    }
}
