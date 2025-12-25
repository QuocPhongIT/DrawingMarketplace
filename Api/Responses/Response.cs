//using System.Text.Json.Serialization;

//namespace DrawingMarketplace.Api.Responses
//{
//    public class Response<T>
//    {
//        public bool Succeeded { get; set; }
//        public string Message { get; set; } = string.Empty;
//        public int StatusCode { get; set; }

//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//        public string[]? Errors { get; set; }

//        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
//        public T? Data { get; set; }

//        public static Response<T> Success(T? data, string message = "Success", int statusCode = 200)
//            => new() { Succeeded = true, Data = data, Message = message, StatusCode = statusCode };

//        public static Response<T> Fail(string message, int statusCode = 400, params string[] errors)
//            => new() { Succeeded = false, Message = message, StatusCode = statusCode, Errors = errors };

//        public static Response<T> Error(params string[] errors)
//            => new() { Succeeded = false, Message = "Internal server error", StatusCode = 500, Errors = errors };
//    }
//}
