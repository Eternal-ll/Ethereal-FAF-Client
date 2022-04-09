using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiUniversalWithRelations : ApiUniversalWithAttributes
    {
        [JsonPropertyName("relationships")]
        public ApiUniversalRelationships Relations { get; set; }
    }
    public class ApiPlayerRelationships
    {
        [JsonPropertyName("avatarAssignments")]
        public ApiUniversalArrayRelationship Avatars { get; set; }
        [JsonPropertyName("clanMembership")]
        public ApiUniversalArrayRelationship ClanMembership { get; set; }
        [JsonPropertyName("names")]
        public ApiUniversalArrayRelationship Names { get; set; }
        [JsonPropertyName("bans")]
        public ApiUniversalArrayRelationship Bans { get; set; }
    }
    public class ApiUniversalWithRelations2 : ApiUniversalWithAttributes
    {
        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }
    }
}
