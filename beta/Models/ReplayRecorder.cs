using beta.Models.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZstdSharp;
using System.IO.Compression;

namespace beta.Models
{
    /// <summary>
    /// This is a simple class that takes all the FA replay data input from
    /// its inputSocket, writes it to a file, and relays it to an internet
    /// server via its relaySocket.
    /// </summary>
    public class ReplayRecorder
    {
        public event EventHandler StreamFinished;

        private readonly TcpClient TcpClient;
        private Socket RelaySocket;

        private Process ForgedAlliance;
        private GameInfoMessage Game;

        public ReplayRecorder(TcpClient tcpClient) => TcpClient = tcpClient;
        public void Initialize(Process forgedAlliance, GameInfoMessage game, string recorder, bool relayToFAF = true)
        {
            game.Command = Server.Enums.ServerCommand.game_info;

            if (relayToFAF)
            {
                try
                {
                    RelaySocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    RelaySocket.Connect("lobby.faforever.com", 15000);
                }
                catch (Exception ex)
                {
                    RelaySocket = null;
                }
            }

            Task.Run(() =>
            {
                var process = forgedAlliance;
                var relay = RelaySocket;
                var client = TcpClient;
                var stream = client.GetStream();
                List<byte> cache = new();
                byte[] last = Array.Empty<byte>();
                try
                {
                    while (client.Connected && !process.HasExited)
                    {
                        if (client.Available == 0)
                        {
                            if (last.Length > 0)
                            {
                                if (RelaySocket is not null && RelaySocket.Connected)
                                {
                                    RelaySocket.Send(last);
                                }
                                //cache.AddRange(last);
                                last = Array.Empty<byte>();
                            }
                            Thread.Sleep(50);
                            continue;
                        }
                        last = new byte[client.Available];
                        stream.Read(last, 0, last.Length);
                    }
                }
                catch { }
                StreamFinished?.Invoke(this, null);
                stream.Dispose();
                client.Close();
                // TODO

                //double gameEnd = DateTimeOffset.Now.ToUnixTimeSeconds();

                //var gameData = GetReplayMetadata(game, recorder, gameEnd) + '\n';

                //if (cache[0] == 80)
                //for (int i = 0; i < cache.Count; i++)
                //{
                //    if (cache[i] == '\x00')
                //    {
                //        cache.RemoveRange(0, i + 1);
                //        break;
                //    }
                //}
                ////var data = Encoding.UTF8.GetString(cache.ToArray());

                ////using var compressor = new Compressor();
                ////var compressed = compressor.Wrap(cache.ToArray());


                //var fileName = $"{game.uid}-{recorder}.fafreplay";
                //var replayPath = "C:\\ProgramData\\FAForever\\replays";
                //if (Directory.Exists(replayPath)) Directory.CreateDirectory(replayPath);

                //File.WriteAllText(replayPath + '\\' + fileName, gameData);
            });
        }
        public string GetReplayMetadata(GameInfoMessage game, string recorder, double gameEnd)
        {
            StringBuilder sb = new();
            sb.Append('{');
            //"uid": 16986178, 
            //"recorder": "Eternal-", 
            //"featured_mod": "faf", 
            //"launched_at": 1651989848.747021, 
            //"complete": true, 
            //"state": "PLAYING", 
            //"num_players": 1, 
            //"max_players": 8, 
            //"title": "Eternal-'s game", 
            //"host": "Eternal-", 
            //"mapname": "scmp_021", 
            //"map_file_path": "maps/scmp_021.zip", 
            //"teams": { "1": ["Eternal-"]}, 
            //"sim_mods": { }, 
            //"password_protected": false,
            //"visibility": "PUBLIC", 
            //"command": "game_info", 
            //"game_end": 1651990035.971194
            sb.Append("\"command\": \"game_info\",");
            sb.Append($"\"uid\": {game.uid},");
            sb.Append($"\"featured_mod\": \"{game.FeaturedMod.ToString().ToLower()}\",");
            sb.Append($"\"launched_at\": {game.launched_at.Value.ToString().Replace(',', '.')},");
            sb.Append($"\"state\": \"{game.State.ToString().ToUpper()}\",");
            sb.Append($"\"num_players\": {game.num_players},");
            sb.Append($"\"max_players\": {game.max_players},");
            sb.Append($"\"title\": \"{game.title}\",");
            sb.Append($"\"host\": \"{game.host}\",");
            sb.Append($"\"mapname\": \"{game.mapname}\",");
            sb.Append($"\"map_file_path\": \"{game.map_file_path}\",");
            sb.Append($"\"teams\": {JsonSerializer.Serialize(game.teams)},");
            sb.Append($"\"sim_mods\": {JsonSerializer.Serialize(game.sim_mods)},");
            sb.Append($"\"password_protected\": {game.password_protected.ToString().ToLower()},");
            sb.Append($"\"visibility\": \"{game.Visibility.ToString().ToUpper()}\",");

            // manual data
            //sb.Append($"\"compression\": \"zstd\",");
            sb.Append($"\"recorder\": \"{recorder}\",");
            sb.Append($"\"complete\": true,");
            sb.Append($"\"game_end\": {gameEnd}}}");
            return sb.ToString();
        }
    }
}
