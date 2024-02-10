using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// Proxy dynamic client for FAForever
    /// </summary>
    /// <remarks>
    /// 
    /// 
    /// Case for GPGNetSend Messages: <br/>
    /// Data flow: game > client > server<br/>
    /// Because this data is dynamic for client and its range contained in game/server,
    /// we have to create this proxy interface client, because NetworkJsonRpc wont generate for us dynamic method
    /// that can be invoked on server.<br/>
    /// Current messages can be seen here:
    /// <see href="https://github.com/FAForever/server/blob/98271c421412467fa387f3a6530fe8d24e360fa4/server/gameconnection.py#L625"/>
    /// </remarks>
    public interface IFafLobbyActionClient
    {
        /// <summary>
        /// Send GPGNetSend message
        /// </summary>
        /// <param name="command">GPGNetSend command</param>
        /// <param name="args">Arguments</param>
        /// <returns></returns>
        public Task SendTargetActionAsync(string command, string target, params object[] args);
    }
}
