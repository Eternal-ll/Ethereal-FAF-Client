﻿using System;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Light.Infrastructure.OAuth
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
    public partial class FafJwtPayload
    {
        [JsonPropertyName("aud")]
        public object[] Aud { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("exp")]
        public long Exp { get; set; }

        [JsonPropertyName("ext")]
        public Ext Ext { get; set; }

        [JsonPropertyName("iat")]
        public long Iat { get; set; }

        [JsonPropertyName("iss")]
        public Uri Iss { get; set; }

        [JsonPropertyName("jti")]
        public Guid Jti { get; set; }

        [JsonPropertyName("nbf")]
        public long Nbf { get; set; }

        [JsonPropertyName("scp")]
        public string[] Scp { get; set; }

        [JsonPropertyName("sub")]
        public string Sub { get; set; }
    }

    public class Ext
    {
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
