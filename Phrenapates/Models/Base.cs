using System.Text.Json.Serialization;

namespace Phrenapates.Models
{
    class BaseResponse
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }
    }
}
