namespace beta.Models.IRC.Enums
{
    /// <summary>
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    internal enum IrcReplyCode
    {
        RPL_FORWARDINGTOCHANNEL = 470,

        RPL_HELP = 705,

        /// <summary>
        /// Welcome to the network.
        /// </summary>
        RPL_WELCOME = 1,

        /// <summary>
        /// Host information.
        /// </summary>
        RPL_YOURHOST = 2,

        /// <summary>
        /// Server creation date.
        /// </summary>
        RPL_CREATED = 3,

        /// <summary>
        /// Server information.
        /// </summary>
        RPL_MYINFO = 4,

        /// <summary>
        /// Server suggests an alternative server to the user.
        /// </summary>
        RPL_BOUNCE = 5,

        /// <summary>
        /// Reply format used by USERHOST.
        /// nickname [ "*" ] "=" ( "+" / "-" ) hostname
        /// The '*' indicates whether the client has registered as an Operator. The '-' or '+' characters represent whether the client has set an AWAY message or not
        /// respectively.
        /// </summary>
        RPL_USERHOST = 302,

        /// <summary>
        /// ISON reply format.
        /// </summary>
        RPL_ISON = 303,

        /// <summary>
        /// User is away notification.
        /// "nick :away message"
        /// </summary>
        RPL_AWAY = 301,

        /// <summary>
        /// You are no longer marked as away.
        /// </summary>
        RPL_UNAWAY = 305,

        /// <summary>
        /// You are now marked as away.
        /// </summary>
        RPL_NOWAWAY = 306,

        /// <summary>
        /// Who is user.
        /// "nick user host * :real name"
        /// </summary>
        RPL_WHOISUSER = 311,

        /// <summary>
        /// Who is server.
        /// "nick server :server info"
        /// </summary>
        RPL_WHOISSERVER = 312,

        /// <summary>
        /// Who is operator.
        /// "nick :is an IRC operator"
        /// </summary>
        RPL_WHOISOPERATOR = 313,

        /// <summary>
        /// Who is idle.
        /// "nick integer :seconds idle"
        /// </summary>
        RPL_WHOISIDLE = 317,

        /// <summary>
        /// End of WHOIS list.
        /// "nick :End of WHOIS list"
        /// </summary>
        RPL_ENDOFWHOIS = 318,

        /// <summary>
        /// The RPL_ENDOFWHOIS reply is used to mark the end of processing a WHOIS message.
        /// "nick :*( ( "@" / "+" ) channel " " )"
        /// </summary>
        RPL_WHOISCHANNELS = 319,

        /// <summary>
        /// "nick user host * :real name"
        /// </summary>
        RPL_WHOWASUSER = 314,

        /// <summary>
        /// "nick :End of WHOWAS"
        /// </summary>
        RPL_ENDOFWHOWAS = 369,

        /// <summary>
        /// "channel #visible :topic"
        /// </summary>
        RPL_LIST = 322,

        /// <summary>
        /// ":End of LIST"
        /// </summary>
        RPL_LISTEND = 323,

        /// <summary>
        /// "channel nickname"
        /// </summary>
        RPL_UNIQOPIS = 325,

        /// <summary>
        /// "channel mode modeparams"
        /// </summary>
        RPL_CHANNELMODEIS = 324,

        /// <summary>
        /// Sent as a reply to the WHOIS command, this numeric indicates that the client with the nickname <nick> was authenticated as the owner of <account>.
        /// This does not necessarily mean the user owns their current nickname, which is covered byRPL_WHOISREGNICK.
        /// </summary>
        RPL_WHOISACCOUNT = 330,

        /// <summary>
        /// Sent to a client when joining a channel to inform them that the channel with the name <channel> does not have any topic set
        /// </summary>
        RPL_NOTOPIC = 331,

        /// <summary>
        /// Sent to a client when joining the <channel> to inform them of the current topic of the channel
        /// <client> <channel> :<topic>
        /// </summary>
        RPL_TOPIC = 332,

        /// <summary>
        /// Sent to a client to let them know who set the topic (<nick>) and when they set it (<setat> is a unix timestamp). Sent after RPL_TOPIC
        /// <client> <channel> <nick> <setat>
        /// </summary>
        RPL_TOPICWHOTIME = 332,

        /// <summary>
        /// Returned by the server to indicate that the attempted INVITE message was successful and is being passed onto the end client.
        /// </summary>
        RPL_INVITING = 341,

        /// <summary>
        /// Summing user to IRC.
        /// </summary>
        RPL_SUMMONING = 342,

        /// <summary>
        /// "channel invitemask"
        /// </summary>
        RPL_INVITELIST = 346,

        RPL_ENDOFINVITELIST = 347,

        /// <summary>
        /// "channel exceptionmask"
        /// </summary>
        RPL_EXCEPTLIST = 348,

        RPL_ENDOFEXCEPTLIST = 349,

        /// <summary>
        /// "version.debuglevel server :comments"
        /// </summary>
        RPL_VERSION = 351,

        /// <summary>
        /// "channel user host server nick ( "H" / "G" > ["*"] [ ( "@" / "+" ) ] :hopcount realname"
        /// </summary>
        RPL_WHOREPLY = 352,

        RPL_ENDOFWHO = 315,

        /// <summary>
        /// "( "=" / "*" / "@" ) channel :[ "@" / "+" ] nick *( " " [ "@" / "+" ] nick )
        /// - "@" is used for secret channels, "*" for private
        ///   channels, and "=" for others (public channels).
        /// </summary>
        RPL_NAMREPLY = 353,

        RPL_ENDOFNAMES = 366,

        RPL_LINKS = 364,

        RPL_ENDOFLINKS = 365,

        /// <summary>
        /// "channel banmask"
        /// </summary>
        RPL_BANLIST = 367,

        RPL_ENDOFBANLIST = 368,

        RPL_INFO = 371,

        RPL_ENDOFINFO = 374,

        /// <summary>
        /// MOTD start.
        /// </summary>
        RPL_MOTDSTART = 375,

        /// <summary>
        /// Part of MOTD.
        /// </summary>
        RPL_MOTD = 372,

        /// <summary>
        /// MOTD end.
        /// </summary>
        RPL_ENDOFMOTD = 376,

        /// <summary>
        /// You are now an IRC operator.
        /// </summary>
        RPL_YOUREOPER = 381,

        /// <summary>
        /// "config file :Rehashing"
        /// </summary>
        RPL_REHASHING = 382,

        /// <summary>
        /// You are a service.
        /// </summary>
        RPL_YOURESERVICE = 383,

        /// <summary>
        /// "server :string showing server's local time"
        /// </summary>
        RPL_TIME = 391,

        RPL_USERSSTART = 392,

        RPL_USERS = 393,

        RPL_ENDOFUSERS = 394,

        /// <summary>
        /// No users are logged in.
        /// </summary>
        RPL_NOUSERS = 395,

        RPL_TRACELINK = 200,
        RPL_TRACECONNECTING = 201,
        RPL_TRACEHANDSHAKE = 202,
        RPL_TRACEUNKNOWN = 203,
        RPL_TRACEOPERATOR = 204,
        RPL_TRACEUSER = 205,
        RPL_TRACESERVER = 206,
        RPL_TRACESERVICE = 207,
        RPL_TRACENEWTYPE = 208,
        RPL_TRACECLASS = 209,
        RPL_TRACERECONNECT = 210,
        RPL_TRACELOG = 261,
        RPL_TRACEEND = 262,

        RPL_STATSLINKINFO = 211,
        RPL_STATSCOMMANDS = 212,
        RPL_ENDOFSTATS = 219,

        /// <summary>
        /// ":Server Up %d days %d:%02d:%02d"
        /// </summary>
        RPL_STATSUPTIME = 242,

        RPL_STATSOLINE = 243,

        RPL_UMODEIS = 221,

        RPL_SERVLIST = 234,

        RPL_SERVLISTEND = 235,

        RPL_LUSERCLIENT = 251,

        RPL_LUSEROP = 252,

        RPL_LUSERUNKNOWN = 253,

        RPL_LUSERCHANNELS = 254,

        RPL_LUSERME = 255,

        RPL_ADMINME = 256,

        RPL_ADMINLOC1 = 257,

        RPL_ADMINLOC2 = 258,

        RPL_ADMINEMAIL = 259,

        RPL_TRYAGAIN = 263,
    }
}
