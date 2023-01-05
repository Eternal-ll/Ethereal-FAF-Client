using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class IceServersData : Base.ServerMessage
    {
        public IceCoturnServer[] ice_servers { get; set; }
        public int ttl { get; set; }
    }

    public class IceCoturnServer
    {
        public IceCoturnServer()
        {

        }
        public IceCoturnServer(string key, string host, int port, int ttl, long playerId)
        {
            Urls = new string[]
            {
                    $"turn:{host}:{port}?transport=tcp",
                    $"turn:{host}:{port}?transport=udp",
                    $"stun:{host}:{port}"
            };
            var tokenName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 + ttl}:{playerId}";
            UserName = tokenName;
            Credential = CreateToken(tokenName, key);
            CredentialType = "token";
        }
        [JsonPropertyName("urls")]
        public string[] Urls { get; set; }
        [JsonPropertyName("username")]
        public string UserName { get; set; }
        [JsonPropertyName("credential")]
        public string Credential { get; set; }
        [JsonPropertyName("credentialType")]
        public string CredentialType { get; set; }
        public static string CreateToken(string message, string secret)
        {
            using var hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(secret));
            return Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(message)));
        }
    }
}
