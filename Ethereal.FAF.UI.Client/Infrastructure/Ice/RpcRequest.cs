using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    internal abstract class Rcp
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; }
    }
    internal class RpcRequest : Rcp
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("params")]
        public List<string> Params { get; set; }
    }
    internal class RpcResult : Rcp
    {
        [JsonPropertyName("result")]
        public object Result { get; set; }
    }
}
