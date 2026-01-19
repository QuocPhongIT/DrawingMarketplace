using System.Text.Json.Serialization;

namespace DrawingMarketplace.Api.Responses
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("message_en")]
        public string MessageEn { get; set; } = string.Empty;
        
        public T? Data { get; set; }
        
        public string Status { get; set; } = "success";
        
        [JsonPropertyName("timeStamp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TimeStamp { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Violation>? Violations { get; set; }
    }
}

