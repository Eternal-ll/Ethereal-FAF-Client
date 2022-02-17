using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace beta.Infrastructure.Services
{
    public class IRCService : IIRCService
    {
        public event EventHandler<EventArgs<string>> Message;
        private readonly ISessionService SessionService;
        //public readonly SSLClient Client = new();
        //public SSLClient SslClient => Client;
        public IRCService(ISessionService lobbySession)
        {
            SessionService = lobbySession;
            SessionService.Authorization += OnLobbyAuthorization;
            //Client.DelimiterDataReceived += OnIRCServerResponse;
        }

        public string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.AppendFormat("{0:x2}", hashBytes[i]);
            }
            return sb.ToString();
        }

        public void OnIRCServerResponse(object sender, string e)
        {
            return;
            Message.Invoke(this, e);
            StringBuilder builder = new();
            string nick = Properties.Settings.Default.PlayerNick;
            string id = Properties.Settings.Default.PlayerId.ToString();
            string password = Properties.Settings.Default.irc_password;
            //if (e.Contains("PING"))
            //{
            //    //PING :357DADF4\r
            //    //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    //    .Append("PONG :")
            //    //    .Append(e.Split(":")[1].Replace("\r", "\r"))
            //    //    .ToString()));
                
                
            //    // PRIVMSG NickServ :identify Eternal- 19050b478205e39b0a92cecb5c248737
            //    Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //        .Append("PASS ")
            //        .Append(password)
            //        .Append("\r\n").ToString()));
            
            //    var t = builder.ToString();
            //    Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //        .Append("NICK ")
            //        .Append(nick)
            //        .Append("\r\n").ToString()));
            
            //    var g = builder.ToString();
            //    Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //        .Append("USER  ")
            //        .Append(id)
            //        .Append(" 0 * :")
            //        .Append(nick)
            //        .Append("\r\n").ToString()));
            //    var gv = CreateMD5(password);
            //    Client.Write(Encoding.UTF8.GetBytes("PRIVMSG NickServ :identify Eternal- " + gv +"\r\n"));
            //    Client.Write(Encoding.UTF8.GetBytes("WHOIS Eternal- \r\n"));
            //    Client.Write(Encoding.UTF8.GetBytes($"identify Eternal- {password} \r\n"));
            //}
            //if (e.Contains("QUIT"))
            //{
            //    auth();
            //}
        }

        private void auth()
        {
            string nick = Properties.Settings.Default.PlayerNick;
            string id = Properties.Settings.Default.PlayerId.ToString();
            string password = Properties.Settings.Default.irc_password;
            StringBuilder builder = new();
            //Client.Write(Encoding.UTF8.GetBytes(builder
            //    .Append("{\"command\":\"auth\",\"token\":\"").Append(accessToken)
            //    .Append("\",\"unique_id\":\"").Append(uid)
            //    .Append("\",\"session\":\"").Append(Session)
            //    .Append("\"}\n").ToString()));

            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("PASS ")
            //    .Append(password)
            //    .Append("\r\n").ToString()));

            //var t = builder.ToString();
            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("NICK ")
            //    .Append(nick)
            //    .Append("\r\n").ToString()));

            //var g = builder.ToString();
            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("USER  ")
            //    .Append(id)
            //    .Append(" 0 * :")
            //    .Append(nick)
            //    .Append("\r\n").ToString()));

            var b = builder.ToString();
        }

        private void OnLobbyAuthorization(object sender, EventArgs<bool> e)
        {
            //Client.Connect("116.202.155.226", 6697);
            //auth();
            //string nick = Properties.Settings.Default.PlayerNick;
            //string id = Properties.Settings.Default.PlayerId.ToString();
            //string password = Properties.Settings.Default.irc_password;
            //StringBuilder builder = new();
            ////Client.Write(Encoding.UTF8.GetBytes(builder
            ////    .Append("{\"command\":\"auth\",\"token\":\"").Append(accessToken)
            ////    .Append("\",\"unique_id\":\"").Append(uid)
            ////    .Append("\",\"session\":\"").Append(Session)
            ////    .Append("\"}\n").ToString()));

            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("PASS ")
            //    .Append(password)
            //    .Append("\r \n  ").ToString()));

            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("NICK ")
            //    .Append(nick)
            //    .Append("\r \n").ToString()));


            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("USER  ")
            //    .Append(id)
            //    .Append(" 0 * :")
            //    .Append(nick)
            //    .Append("\r \n").ToString()));
            //Client.Write(Encoding.UTF8.GetBytes(builder.Clear()
            //    .Append("JOIn #aeolus\r \n").ToString()));

            // PASS a5960a4440a3045b4cf025983f0d3f5c

            // PASS a5960a4440a3045b4cf025983f0d3f5c b'\r\n'

            // NICK Eternal-

            // QUIT Connection reset by peer

            // USER {id} 0 * :{playerNick}

            // JOIN #aeolus
        }
    }
}
