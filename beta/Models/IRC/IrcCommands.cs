using beta.Models.IRC.Enums;

namespace beta.Models.IRC
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/List_of_Internet_Relay_Chat_commands
    /// https://modern.ircdocs.horse/index.html
    /// http://www.faqs.org/rfcs/rfc2812.html
    /// </summary>
    internal static class IrcCommands
    {
        /// <summary>
        /// Sets a connection password. This command must be sent before the NICK/USER registration combination
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static string Pass(string password) => "PASS " + password;
        /// <summary>
        /// The NICK command is used to give the client a nickname or change the previous on
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns></returns>
        internal static string Nickname(string nickname) => "NICK " + nickname;
        /// <summary>
        /// This command is used at the beginning of a connection to specify the username, real name
        /// and initial user modes of the connecting client.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="readlName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        internal static string UserInfo(string username, string readlName, IrcUserFlags mode = IrcUserFlags.NORMAL) =>
            "USER " + username + " " + (byte)mode + " * :" + readlName;
        /// <summary>
        /// Makes the client join the channel, specifying the password, if needed.
        /// If the channel do not exist then it will be created
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
        internal static string Join(string channel, string key = null) => "JOIN " + channel + " " + key;
        /// <summary>
        /// Makes the client join the channels in the comma-separated list <channels>, specifying the passwords,
        /// if needed, in the comma-separated list <keys>. If the channel(s) do not exist then they will be created
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        internal static string Join(string[] channels, string[] keys = null) =>
            "JOIN " + string.Join(',', channels) + " " + keys is not null ? string.Join(',', keys) : null;
        /// <summary>
        /// Causes a user to leave the channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        internal static string Leave(string channels) => "PART " + channels;
        /// <summary>
        /// Causes a user to leave the channels in the comma-separated list <channels>
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        internal static string Leave(string[] channels) => "PART " + string.Join(',', channels);
        /// <summary>
        /// Sends <message> to <msgtarget>, which is usually a user or channel
        /// </summary>
        /// <param name="msgtarget"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string Message(string msgtarget, string message) => "PRIVMSG " + msgtarget + " :" + message;
        /// <summary>
        /// This command works similarly to PRIVMSG (Message), except automatic replies must never be sent in reply to NOTICE messages
        /// </summary>
        /// <param name="msgtarget"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static string Notice(string msgtarget, string message) => "PRIVMSG " + msgtarget + " :" + message;
        /// <summary>
        /// Leaves from all joined channels by command JOIN 0
        /// </summary>
        /// <returns></returns>
        internal static string LeaveAllJoinedChannels() => "JOIN 0";
        /// <summary>
        /// Disconnects the user from the server
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        internal static string Quit(string reason = null) => "QUIT :" + reason;
        /// <summary>
        /// Tests the presence of a connection. A PING message results in a PONG reply.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        internal static string Ping(string cookie = null) => "PING :" + cookie;
        /// <summary>
        /// This command is a reply to the PING command and works in much the same way
        /// </summary>
        /// <returns></returns>
        internal static string Pong(string cookie = null) => "PONG :" + cookie;
        /// <summary>
        /// Allows the client to query or set the channel topic on <channel>. If <topic> is given,
        /// it sets the channel topic to <topic>. If channel mode +t is set, only a channel operator may set the topic
        ///   TOPIC #test :New topic          ; Setting the topic on "#test" to "New topic".
        ///   TOPIC #test :                   ; Clearing the topic on "#test"
        ///   TOPIC #test                     ; Checking the topic for "#test"
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        internal static string Topic(string channel, string topic = null) => "TOPIC " + channel + " :" + topic;
        /// <summary>
        /// Returns statistics about the current server, or <server> if it's specified.
        /// c - returns a list of servers which the server may connect to or allow connections from;
        /// h - returns a list of servers which are either forced to be treated as leaves or allowed to act as hubs;
        /// i - returns a list of hosts which the server allows a client to connect from;
        /// k - returns a list of banned username/hostname combinations for that server;
        /// l - returns a list of the server’s connections, showing how long each connection has been established and the traffic over that connection in bytes and messages for each direction;
        /// m - returns a list of commands supported by the server and the usage count for each if the usage count is non zero;
        /// o - returns a list of hosts from which normal clients may become operators;
        /// u - returns a string showing how long the server has been up.
        /// y - show Y (Class) lines from server’s configuration file;
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        internal static string Stats(string query, string server = null) => "STATS " + query + " " + server;
        /// <summary>
        /// Forcibly removes <user> from <channel>.This command may only be issued by channel operators.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="user"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        internal static string Kick(string channel, string user, string comment = null) => "KICK " + channel + " " + user + " " + comment;
        /// <summary>
        /// Forcibly removes <users> from <channel>.This command may only be issued by channel operators.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="users"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        internal static string Kick(string channel, string[] users, string comment = null) => "KICK " + channel + " " + string.Join(',', users) + " " + comment;
        /// <summary>
        /// Invites <user> to the channel <channel>. <channel> does not have to exist, but if it does, only members of the channel are allowed to invite other clients.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        internal static string Invite(string user, string channel) => "INVITE " + user + " " + channel;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumUsers"></param>
        /// <returns></returns>
        internal static string List(int minimumUsers = 2) => "LIST >" + minimumUsers;
    }
}
