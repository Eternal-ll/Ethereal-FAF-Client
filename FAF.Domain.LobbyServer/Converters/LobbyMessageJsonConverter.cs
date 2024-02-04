using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Converters
{
    public class LobbyMessageJsonConverter : JsonConverter<ServerMessage>
	{
		private static Dictionary<ServerCommand, Func<JsonDocument, ServerMessage>> _commandConverters;
        public LobbyMessageJsonConverter()
        {
			_commandConverters = new Dictionary<ServerCommand, Func<JsonDocument, ServerMessage>>()
			{
				{ ServerCommand.unknown,
					(json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.auth, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.authentication_failed, (json) => JsonSerializer.Deserialize<AuthentificationFailedData>(json) }, 
				//{ ServerCommand.ask_session, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.session, (json) => JsonSerializer.Deserialize<SessionMessage>(json) }, 
				{ ServerCommand.notice, (json) => JsonSerializer.Deserialize<Notification>(json) }, 
				{ ServerCommand.irc_password, (json) => JsonSerializer.Deserialize<IrcPassword>(json) }, 
				{ ServerCommand.welcome, (json) => JsonSerializer.Deserialize<WelcomeData>(json) }, 
				{ ServerCommand.social, (json) => JsonSerializer.Deserialize<SocialData>(json) }, 
				//{ ServerCommand.player_info, (json) => JsonSerializer.Deserialize<PlayerInfoMessage>(json) }, 
				//{ ServerCommand.game_info, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.game, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.matchmaker_info, (json) => JsonSerializer.Deserialize<MatchmakingData>(json) }, 
				//{ ServerCommand.mapvault_info, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.ping, (json) => JsonSerializer.Deserialize<PingMessage>(json) }, 
				{ ServerCommand.pong, (json) => JsonSerializer.Deserialize<PongMessage>(json) }, 
				{ ServerCommand.game_launch, (json) => JsonSerializer.Deserialize<GameLaunchData>(json) }, 
				{ ServerCommand.party_invite, (json) => JsonSerializer.Deserialize<PartyInvite>(json) }, 
				{ ServerCommand.update_party, (json) => JsonSerializer.Deserialize<PartyUpdate>(json) }, 
				{ ServerCommand.invite_to_party, (json) => JsonSerializer.Deserialize<PartyInvite>(json) }, 
				{ ServerCommand.kicked_from_party, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.set_party_factions, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.match_info, (json) => JsonSerializer.Deserialize<MatchConfirmation>(json) }, 
				//{ ServerCommand.match_ready, (json) => JsonSerializer.Deserialize<UnknomawnServerMessage>(json) }, 
				{ ServerCommand.match_found, (json) => JsonSerializer.Deserialize<MatchFound>(json) }, 
				{ ServerCommand.match_cancelled, (json) => JsonSerializer.Deserialize<MatchCancelled>(json) }, 
				{ ServerCommand.search_info, (json) => JsonSerializer.Deserialize<SearchInfo>(json) },
				//{ ServerCommand.restore_game_session, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.game_matchmaking, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.game_host, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.game_join, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.ice_servers, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.invalid, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				{ ServerCommand.JoinGame, (json) => JsonSerializer.Deserialize<IceUniversalData2>(json) }, 
				{ ServerCommand.HostGame, (json) => JsonSerializer.Deserialize<IceUniversalData2>(json) }, 
				{ ServerCommand.ConnectToPeer, (json) => JsonSerializer.Deserialize<IceUniversalData2>(json) }, 
				{ ServerCommand.DisconnectFromPeer, (json) => JsonSerializer.Deserialize<IceUniversalData2>(json) }, 
				{ ServerCommand.IceMsg, (json) => JsonSerializer.Deserialize<IceUniversalData2>(json) }, 
				//{ ServerCommand.GameFull, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.ClearSlot, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
				//{ ServerCommand.Chat, (json) => JsonSerializer.Deserialize<UnknownServerMessage>(json) }, 
			};
        }
		public override bool CanConvert(Type typeToConvert)
		{
			return typeToConvert == typeof(ServerMessage) ||
				typeToConvert.IsSubclassOf(typeof(ServerMessage));
		}
		public override ServerMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (!JsonDocument.TryParseValue(ref reader, out var doc))
				throw new JsonException("Недействительный JSON объект");
			if (!doc.RootElement.TryGetProperty("command", out var commandText))
				throw new JsonException("Недействительный JSON объект");
			if (!Enum.TryParse<ServerCommand>(commandText.ToString(), out var command))
			{
				return new UnknownServerMessage()
				{
					Command = ServerCommand.unknown,
					ServerCommand = commandText.ToString()
				};
			}
			if (command == ServerCommand.game_info)
			{
				if (doc.RootElement.TryGetProperty("games", out _))
				{
					return JsonSerializer.Deserialize<LobbyGames>(doc);
				}
				return JsonSerializer.Deserialize<GameInfoMessage>(doc);
			}
			else if (command == ServerCommand.player_info)
			{
				if (doc.RootElement.TryGetProperty("players", out _))
				{
					return JsonSerializer.Deserialize<LobbyPlayers>(doc);
				}
				return JsonSerializer.Deserialize<PlayerInfoMessage>(doc);
			}
			return _commandConverters[command](doc);
		}

		public override void Write(Utf8JsonWriter writer, ServerMessage value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
