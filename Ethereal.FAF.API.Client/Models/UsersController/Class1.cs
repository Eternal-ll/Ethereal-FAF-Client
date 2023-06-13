using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.UsersController
{
    public partial class RequestError
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }

        [JsonPropertyName("errors")]
        public Error[] Errors { get; set; }
    }

    public partial class Error
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}
