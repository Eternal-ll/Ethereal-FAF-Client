using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace beta.Infrastructure.Converters.JSON
{
    public class RawStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            return jsonDoc.RootElement.GetRawText();
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    //"version": "SNAPSHOT",
    //"ice_servers_size": 0,
    //"lobby_port": 21374,
    //"init_mode": "normal",
    //"options": {
    //  "player_id": 3,
    //  "player_login": "test",
    //  "rpc_port": 55121,
    //  "gpgnet_port": 53197
    //},
    //"gpgpnet": {
    //  "local_port": 53197,
    //  "connected": false,
    //  "game_state": "",
    //  "task_string": "-"
    //},
    //"relays": []
}
