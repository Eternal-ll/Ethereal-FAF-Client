using System.Text.Json.Serialization;

namespace beta.Models.API
{
    public class ApiVaultPage
    {
        [JsonPropertyName("limit")]
        public int Size { get; set; }
        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
        [JsonPropertyName("number")]
        public int PageNumber { get; set; }
        [JsonPropertyName("totalPages")]
        public int AvaiablePagesCount { get; set; }
    }
}
