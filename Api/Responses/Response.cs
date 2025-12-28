using System.Text.Json.Serialization;

namespace DrawingMarketplace.Api.Responses
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        public static Response<T> Ok(
            T? data,
            string message = "OK",
            int statusCode = StatusCodes.Status200OK)
        {
            return new Response<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }
    }
}

