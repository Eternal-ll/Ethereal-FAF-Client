namespace Ethereal.FAF.UI.Client.Models.IRC.Enums
{
    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    internal enum IrcErrorReplyCode
    {
        /// <summary>
        /// The nickname parameter in a command is currently unused.
        /// </summary>
        ERR_NOSUCHNICK = 401,

        /// <summary>
        /// The given server name does not exist.
        /// </summary>
        ERR_NOSUCHSERVER = 402,

        /// <summary>
        /// The given channel does not exist.
        /// </summary>
        ERR_NOSUCHCHANNEL = 403,

        /// <summary>
        /// Cannot send to channel.
        /// </summary>
        ERR_CANNOTSENDTOCHAN = 404,

        /// <summary>
        /// You have joined too many channels.
        /// </summary>
        ERR_TOOMANYCHANNELS = 405,

        /// <summary>
        /// Returned by WHOWAS to indicate there is no history for that nickname.
        /// </summary>
        ERR_WASNOSUCHNICK = 406,

        /// <summary>
        /// Too many private message targets.
        /// </summary>
        ERR_TOOMANYTARGETS = 407,

        /// <summary>
        /// Tried to send SQUERY to unexisting service.
        /// </summary>
        ERR_NOSUCHSERVICE = 408,

        /// <summary>
        /// PING or PONG message missing the originator parameter.
        /// </summary>
        ERR_NOORIGIN = 409,

        ERR_NORECIPIENT = 411,

        ERR_NOTEXTTOSEND = 412,

        ERR_NOTOPLEVEL = 413,

        ERR_WILDTOPLEVEL = 414,

        ERR_BADMASK = 415,

        /* 412 - 415 are returned by PRIVMSG to indicate that
           the message wasn't delivered for some reason.
           ERR_NOTOPLEVEL and ERR_WILDTOPLEVEL are errors that
           are returned when an invalid use of
           "PRIVMSG $<server>" or "PRIVMSG #<host>" is attempted.
         */

        ERR_UNKNOWNCOMMAND = 421,

        /// <summary>
        /// Server could not open MOTD file.
        /// </summary>
        ERR_NOMOTD = 422,

        ERR_NOADMININFO = 423,

        ERR_FILEERROR = 424,

        /// <summary>
        /// Returned when a nickname parameter expected for a command and isn't found.
        /// </summary>
        ERR_NONICKNAMEGIVEN = 431,

        /// <summary>
        /// Erroneous nickname. Returned after receiving a NICK message which contains characters which do not fall in the defined set.
        /// </summary>
        ERR_ERRONEUSNICKNAME = 432,

        /// <summary>
        /// Nickname is occupied.
        /// </summary>
        ERR_NICKNAMEINUSE = 433,

        ERR_NICKCOLLISION = 436,

        /// <summary>
        /// The command was blocked by the server delay mechanism.
        /// </summary>
        ERR_UNAVAILRESOURCE = 437,

        /// <summary>
        /// The target user of the command is not in the channel.
        /// </summary>
        ERR_USERNOTINCHANNEL = 441,

        /// <summary>
        /// You are not in that channel.
        /// </summary>
        ERR_NOTONCHANNEL = 442,

        /// <summary>
        /// The invited user is already in this channel.
        /// </summary>
        ERR_USERONCHANNEL = 443,

        /// <summary>
        /// SUMMON was unable to be carried out since they are not logged in.
        /// </summary>
        ERR_NOLOGIN = 444,

        /// <summary>
        /// SUMMON has been disabled.
        /// </summary>
        ERR_SUMMONDISABLED = 445,

        /// <summary>
        /// USERS has been disabled.
        /// </summary>
        ERR_USERSDISABLED = 446,

        /// <summary>
        /// The client MUST be registered.
        /// </summary>
        ERR_NOTREGISTERED = 451,

        /// <summary>
        /// Need more parameters in command.
        /// </summary>
        ERR_NEEDMOREPARAMS = 461,

        /// <summary>
        /// Server blocks second USER attempt.
        /// </summary>
        ERR_ALREADYREGISTRED = 462,

        ERR_NOPERMFORHOST = 463,

        /// <summary>
        /// Password is incorrect.
        /// </summary>
        ERR_PASSWDMISMATCH = 464,

        /// <summary>
        /// You have been banned from that connection.
        /// </summary>
        ERR_YOUREBANNEDCREEP = 465,

        /// <summary>
        /// Your connection will soon be denied.
        /// </summary>
        ERR_YOUWILLBEBANNED = 466,

        ERR_KEYSET = 467,

        ERR_CHANNELISFULL = 471,

        ERR_UNKNOWNMODE = 472,

        /// <summary>
        /// Invite-only channel.
        /// </summary>
        ERR_INVITEONLYCHAN = 473,

        ERR_BANNEDFROMCHAN = 474,

        ERR_BADCHANNELKEY = 475,

        ERR_BADCHANMASK = 476,

        ERR_NOCHANMODES = 477,

        /// <summary>
        /// Channel list is full.
        /// </summary>
        ERR_BANLISTFULL = 478,

        /// <summary>
        /// You're not an operator user.
        /// </summary>
        ERR_NOPRIVILEGES = 481,

        /// <summary>
        /// You need to be channel operator to do that.
        /// </summary>
        ERR_CHANOPRIVSNEEDED = 482,

        /// <summary>
        /// You can't kill a server!
        /// </summary>
        ERR_CANTKILLSERVER = 483,

        /// <summary>
        /// Your connection is restricted.
        /// </summary>
        ERR_RESTRICTED = 484,

        /// <summary>
        /// You're not the original channel operator.
        /// </summary>
        ERR_UNIQOPPRIVSNEEDED = 485,

        ERR_NOOPERHOST = 491,

        /// <summary>
        /// Unknown MODE flag.
        /// </summary>
        ERR_UMODEUNKNOWNFLAG = 501,

        /// <summary>
        /// Cannot change mode for other users.
        /// </summary>
        ERR_USERSDONTMATCH = 502,
    }
}
