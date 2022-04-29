using beta.Infrastructure.Converters.JSON;
using System.Text.Json.Serialization;

namespace beta.Models.Ice.Base
{
    public class JsonRpcData : JsonRpc
    {
        public string result { get; set; }
        public JsonRpcErrorData error { get; set; }
        public int id { get; set; }
    }
    public class JsonRpcInvalidData : JsonRpc
    {
        public JsonRpcErrorData error { get; set; }
        public int? id { get; set; }
    }
    public class JsonRpcMethodData : JsonRpcMethod
    {
        public override string method { get; set; }
        [JsonPropertyName("params")]
        [JsonConverter(typeof(RawStringConverter))]
        public string Params { get; set; }
    }
    public class JsonRpcIceServers : JsonRpcMethod
    {
        public override string method { get; set; } = "setIceServers";
        [JsonPropertyName("params")]
        public string[] Params { get; set; } = new string[1];
        //public JsonRpcIceServers(IceServerData[] servers)
        //{
        //    StringBuilder sb = new();
        //    sb.Append('[');
        //    for (int i = 0; i < servers.Length; i++)
        //    {
        //        var serverJson = JsonSerializer.Serialize(servers[i]);
        //        sb.Append(serverJson);

        //        if (i < servers.Length - 1) sb.Append(',');
        //    }
        //    sb.Append(']');
        //    Params[0] = sb.ToString();
        //}
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
