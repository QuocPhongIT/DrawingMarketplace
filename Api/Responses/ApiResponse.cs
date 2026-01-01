using System.Text.Json.Serialization;

namespace DrawingMarketplace.Api.Responses
{
    public interface IApiResponse<T>
    {
        string Message { get; set; }
        string MessageEn { get; set; }
        T? Data { get; set; }
        string Status { get; set; }
        string? TimeStamp { get; set; }
        List<Violation>? Violations { get; set; }
        bool? LimitReached { get; set; }
        int? DownloadCount { get; set; }
        int? RemainingTime { get; set; }
    }

    public class ApiResponse<T> : IApiResponse<T>
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
        
        [JsonPropertyName("limitReached")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? LimitReached { get; set; }
        
        [JsonPropertyName("downloadCount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? DownloadCount { get; set; }
        
        [JsonPropertyName("remainingTime")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? RemainingTime { get; set; }
    }
}

