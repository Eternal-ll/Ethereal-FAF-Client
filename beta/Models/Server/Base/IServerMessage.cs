using beta.Models.Server.Enums;

namespace beta.Models.Server.Base
{
    public interface IServerMessage
    {
        //public string command { get; set; }

        //[JsonPropertyName("command")]
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public ServerCommand Command { get; set; }
    }
}
