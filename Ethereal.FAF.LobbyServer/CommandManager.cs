using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using System.Text.Json;

namespace Ethereal.FAF.LobbyServer
{
    public class AuthData
    {
        public string token { get; set; }
        public string unique_id { get; set; } 
        public long session { get; set; }
    }
    public class CommandManager
    {
        private readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };
        public async Task<string?> UniversalAsync(ServerCommand command, string json) => command switch
        {
            ServerCommand.auth => auth(json),
            ServerCommand.ask_session => ask_session(json),
            ServerCommand.authentication_failed => throw new NotImplementedException(),
            ServerCommand.notice => throw new NotImplementedException(),
            ServerCommand.session => throw new NotImplementedException(),
            ServerCommand.irc_password => throw new NotImplementedException(),
            ServerCommand.welcome => throw new NotImplementedException(),
            ServerCommand.social => throw new NotImplementedException(),
            ServerCommand.player_info => throw new NotImplementedException(),
            ServerCommand.game_info => throw new NotImplementedException(),
            ServerCommand.game => throw new NotImplementedException(),
            ServerCommand.matchmaker_info => throw new NotImplementedException(),
            ServerCommand.mapvault_info => throw new NotImplementedException(),
            ServerCommand.ping => throw new NotImplementedException(),
            ServerCommand.pong => throw new NotImplementedException(),
            ServerCommand.game_launch => throw new NotImplementedException(),
            ServerCommand.party_invite => throw new NotImplementedException(),
            ServerCommand.update_party => throw new NotImplementedException(),
            ServerCommand.invite_to_party => throw new NotImplementedException(),
            ServerCommand.kicked_from_party => throw new NotImplementedException(),
            ServerCommand.set_party_factions => throw new NotImplementedException(),
            ServerCommand.match_found => throw new NotImplementedException(),
            ServerCommand.match_cancelled => throw new NotImplementedException(),
            ServerCommand.search_info => throw new NotImplementedException(),
            ServerCommand.restore_game_session => throw new NotImplementedException(),
            ServerCommand.game_matchmaking => throw new NotImplementedException(),
            ServerCommand.game_host => throw new NotImplementedException(),
            ServerCommand.game_join => throw new NotImplementedException(),
            ServerCommand.ice_servers => throw new NotImplementedException(),
            ServerCommand.invalid => throw new NotImplementedException(),
            ServerCommand.JoinGame => throw new NotImplementedException(),
            ServerCommand.HostGame => throw new NotImplementedException(),
            ServerCommand.ConnectToPeer => throw new NotImplementedException(),
            ServerCommand.DisconnectFromPeer => throw new NotImplementedException(),
            ServerCommand.IceMsg => throw new NotImplementedException(),
            _=> throw new NotImplementedException()
        };

        private string auth(string json)
        {
            var data = JsonSerializer.Deserialize<AuthData>(json);
            return JsonSerializer.Serialize(new WelcomeData()
            {
                Command = ServerCommand.welcome,
                me = new()
                {
                    login = data.unique_id[..10],
                    id = new Random().Next(0, 1000000),
                    ratings = new Dictionary<string, Rating>()
                     {
                         { "global", new Rating()
                         {
                             name = "global",
                              number_of_games = 500,
                              rating= new double[]{1500, 500}
                         }}
                     }
                },
                id = new Random().Next(0, 1000000),
                login = data.unique_id[..10]
            }, JsonSerializerOptions);
        }

        public string ask_session(string json)
        {
            return $"{{\"command\":\"session\",\"session\":{new Random().Next(100000, 1000000)}}}";
        }
    }
}
