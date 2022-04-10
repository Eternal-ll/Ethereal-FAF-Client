using System.Text.Json.Serialization;

namespace beta.Models.Ice
{
    /// <summary>
    /// It describes the current state of the ICE agent and its connection to the ICE server;
    /// that is, the STUN or TURN server.
    /// https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceConnectionState
    /// </summary>
    internal enum IceState : byte
    {
        /// <summary>
        /// The ICE agent is gathering addresses or is waiting to be given remote candidates
        /// through calls to RTCPeerConnection.addIceCandidate() (or both).
        /// </summary>
        New,
        Checking,
        Connected,
        /// <summary>
        /// The ICE agent has finished gathering candidates,
        /// has checked all pairs against one another,
        /// and has found a connection for all components.
        /// </summary>
        Completed,
        Failed,
        Disconnected,
        /// <summary>
        /// The ICE agent for this RTCPeerConnection has shut down and is no longer handling requests.
        /// </summary>
        Closed
    }
    internal abstract class IceData
    {
        [JsonPropertyName("localPlayedId")]
        public int LocalPlayedId { get; set; }

        [JsonPropertyName("remotePlayedId")]
        public int RemotePlayedId { get; set; }
    }
    internal class IceMessage : IceData
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; }
    }
    internal class IceConnectionStateData : IceData
    {
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IceState State { get; set; }
    }
    internal class IcePeerConnectionStateData : IceData
    {

        [JsonPropertyName("connected")]
        public bool IsConnected { get; set; }
    }
    internal class GpgNetMessage
    {
        public GpgNetMessage(string command, string args)
        {
            Command = command;
            Args = args;
        }

        public string Command { get; }
        public string Args { get; }

    }
}
