using System;
using System.Text.Json.Serialization;

namespace beta.Models.OAuth
{
    public class TokenBearer
    {
        //  "access_token": "**********",
        //    "expires_in": "**********",
        //      "id_token": "**********",
        // "refresh_token": "***********",
        //         "scope": "openid offline public_profile lobby",
        //    "token_type": "bearer"

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public double ExpiresIn { get; set; }
        public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
