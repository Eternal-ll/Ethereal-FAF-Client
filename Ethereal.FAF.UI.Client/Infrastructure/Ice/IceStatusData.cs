namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class SnapshotOptions
    {
        public int player_id { get; set; }
        public string player_login { get; set; }
        public int rpc_port { get; set; }
        public int gpgnet_port { get; set; }
    }
    public class SnapshotGpgNet
    {
        public int local_port { get; set; }
        public bool connected { get; set; }
        public string game_state { get; set; }
        public string task_string { get; set; }
    }
    /// <summary>
    /// Connection's ICE gathering state. This lets you detect, for example, when collection of ICE candidates has finished.
    /// https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceGatheringState
    /// </summary>
    internal enum IceGatheringState : byte
    {
        /// <summary>
        /// The peer connection was just created and hasn't done any networking yet.
        /// </summary>
        New,
        /// <summary>
        /// The ICE agent is in the process of gathering candidates for the connection.
        /// </summary>
        Gathering,
        /// <summary>
        /// The ICE agent has finished gathering candidates. If something happens that requires collecting new candidates,
        /// such as a new interface being added or the addition of a new ICE server, the state will revert to gathering to gather those candidates.
        /// </summary>
        Complete
    }
    /// <summary>
    /// see https://developer.mozilla.org/en-US/docs/Web/API/RTCDataChannel/readyState
    /// </summary>
    internal enum RTCDataChannelState : byte
    {
        /// <summary>
        /// 
        /// </summary>
        Connecting,
        Open,
        Closing,
        Closed
    }
    /// <summary>
    /// The type of the peer? candidate
    /// </summary>
    internal enum IceCandidateType : byte
    {
        Local,
        Stun,
        Relay
    }
    public class IcePeerStateData
    {
        /// <summary>
        /// one peer is always offerer, one answerer
        /// </summary>
        public bool offerer { get; set; }
        /// <summary>
        /// The connection state https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceConnectionState
        /// </summary>
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public string state { get; set; }
        /// <summary>
        /// The gathering state https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/iceGatheringState
        /// </summary>
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public string gathering_state { get; set; }
        /// <summary>
        /// Returns a string which indicates the state of the data channel's underlying data connection. It can have on of the following values: connecting, open, closing, or closed.
        /// </summary>
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public string datachannel_state { get; set; }
        /// <summary>
        /// Is the peer connected? Needs to be in sync with the remote peer.
        /// </summary>
        public bool connected { get; set; }
        /// <summary>
        /// The local address used for the connection
        /// </summary>
        public string loc_cand_addr { get; set; }
        /// <summary>
        /// The remote address used for the connection
        /// </summary>
        public string rem_cand_addr { get; set; }
        /// <summary>
        /// The type of the local candidate
        /// </summary>
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public string loc_cand_type { get; set; }
        /// <summary>
        /// The type of the remote candidate
        /// </summary>
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public string rem_cand_type { get; set; }
        /// <summary>
        /// The time it took to connect to the peer in seconds
        /// </summary>
        public double time_to_connected { get; set; }
    }
    internal class RelayData
    {
        public int remote_player_id { get; set; }
        public string remote_player_login { get; set; }
        public int local_game_udp_port { get; set; }
    }
    public class IceStatusData
    {
        /*
        "{"version":"SNAPSHOT",
          "ice_servers_size":0,
          "lobby_port":54135,
          "init_mode":"normal",
          "options":{"player_id":302176,"player_login":"Eternal-","rpc_port":55251,"gpgnet_port":32721},
          "gpgpnet":{"local_port":32721,"connected":false,"game_state":"","task_string":"-"},
          "relays":[]}",
        */
        /// <summary>
        /// faf-ice-adapter version
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// the number of ICE servers set using `setIceServers`
        /// </summary>
        public int ice_servers_size { get; set; }
        /// <summary>
        /// the actual game lobby UDP port. Should match --lobby-port option if non-zero port is specified.
        /// </summary>
        public int lobby_port { get; set; }
        /// <summary>
        /// the current init mode. See the current init mode. See setLobbyInitMode
        /// </summary>
        public string init_mode { get; set; }
        /// <summary>
        /// The specified commandline options
        /// </summary>
        public SnapshotOptions options { get; set; }
        /// <summary>
        /// The GPGNet state
        /// </summary>
        public SnapshotGpgNet gpgpnet { get; set; }
        /// <summary>
        /// An array of relay information for each peer
        /// </summary>
        public IcePeerStateData[] relays { get; set; }
    }
}
