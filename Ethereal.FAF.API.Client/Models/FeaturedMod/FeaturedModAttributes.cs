using Ethereal.FAF.API.Client.Models.Base;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.FeaturedMod
{
    public class EntityResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
    public class Entity<T>
    {
        [JsonPropertyName("id")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long Id { get; set; }
        [JsonPropertyName("type")]
        public string Type{ get; set; }
        [JsonPropertyName("attributes")]
        public T Attributes { get; set; }
        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }
        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }
        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
    public class FeaturedModEntity : Entity<FeaturedModAttributes>
    {

    }
    public class FeaturedModsResponse : EntityResponse<FeaturedModEntity[]>
    {

    }
    public class FeaturedModAttributes
    {
        [JsonPropertyName("allowOverride")]
        public bool? AllowOverride { get; set; } = false;
        [JsonPropertyName("deploymentWebhook")]
        public string DeploymentWebhook { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
        [JsonPropertyName("fileExtension")]
        public string FileExtension { get; set; }
        [JsonPropertyName("gitBranch")]
        public string GitBranch { get; set; }
        [JsonPropertyName("gitUrl")]
        public string GitUrl { get; set; }
        [JsonPropertyName("order")]
        public int Order { get; set; }
        [JsonPropertyName("technicalName")]
        public string TechnicalName { get; set; }
        [JsonPropertyName("visible")]
        public bool Visible { get; set; }
    }
}
