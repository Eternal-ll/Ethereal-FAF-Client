using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    /// <summary>
    /// An array of objects, each describing one server which may be used by the ICE agent; these are typically STUN and/or TURN servers. If this isn't specified, the connection attempt will be made with no STUN or TURN server available, which limits the connection to local peers. Each object may have the following properties:
    /// </summary>
    public class IceServer
    {
        /// <summary>
        /// The credential to use when logging into the server. This is only used if the object represents a TURN server.
        /// </summary>
        [JsonPropertyName("credential")]
        public string Credential { get; set; }
        /// <summary>
        /// If the object represents a TURN server, this attribute specifies what kind of credential is to be used when connecting. The default is "password".
        /// </summary>
        [JsonPropertyName("credentialType")]
        public string CredentialType { get; set; }
        /// <summary>
        /// This required property is either a single string or an array of strings, each specifying a URL which can be used to connect to the server.
        /// </summary>
        [JsonPropertyName("urls")]
        public string[] Urls { get; set; }
        /// <summary>
        /// If the object represents a TURN server, then this is the username to use during the authentication.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
