namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
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
}
