using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace DrawingMarketplace.Api.Responses
{
    public static class ResponseHelper
    {
        private static string GetTimeStamp()
        {
            var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vietnamTz);
            return vietnamTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static async Task WriteErrorResponseAsync(
            HttpContext context,
            int statusCode,
            string message,
            string messageEn,
            List<Violation>? violations = null,
            string status = "fail")
        {
            var response = new ApiResponse<object>
            {
                Message = message,
                MessageEn = messageEn,
                Data = null,
                Status = status,
                TimeStamp = GetTimeStamp(),
                Violations = violations
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, jsonOptions));
        }

        public static ObjectResult CreateResponse<T>(
            ControllerBase controller,
            int statusCode,
            string message,
            string messageEn,
            T? data = default,
            string status = "success",
            List<Violation>? violations = null)
        {
            var response = new ApiResponse<T>
            {
                Message = message,
                MessageEn = messageEn,
                Data = data,
                Status = status,
                TimeStamp = GetTimeStamp(),
                Violations = violations
            };

            return controller.StatusCode(statusCode, response);
        }

        public static ObjectResult SuccessResponse<T>(
            ControllerBase controller,
            int statusCode,
            string message,
            T? data = default,
            string? messageEn = null)
        {
            return CreateResponse(
                controller,
                statusCode,
                message,
                messageEn ?? message,
                data,
                "success"
            );
        }

        public static ObjectResult FailResponse(
            ControllerBase controller,
            int statusCode,
            string message,
            string? messageEn = null,
            List<Violation>? violations = null)
        {
            return CreateResponse<object>(
                controller,
                statusCode,
                message,
                messageEn ?? message,
                null,
                "fail",
                violations
            );
        }
        public static ObjectResult InvalidCredentialsError(ControllerBase controller)
        {
            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    En = "Username or password is incorrect",
                    Vi = "Tên đăng nhập hoặc mật khẩu không chính xác"
                },
                Type = "InvalidCredentials",
                Code = 401
            };

            return FailResponse(
                controller,
                401,
                "Tên đăng nhập hoặc mật khẩu không chính xác",
                "Username or password is incorrect",
                new List<Violation> { violation }
            );
        }

        public static ObjectResult AccountNotActiveError(ControllerBase controller)
        {
            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    En = "Account is not activated",
                    Vi = "Tài khoản chưa được kích hoạt"
                },
                Type = "AccountNotActive",
                Code = 403
            };

            return FailResponse(
                controller,
                403,
                "Tài khoản chưa được kích hoạt",
                "Account is not activated",
                new List<Violation> { violation }
            );
        }

        public static ObjectResult DuplicateEntryError(ControllerBase controller, string field)
        {
            var (message, messageEn) = field.ToLower() == "email"
                ? ("Email đã được sử dụng", "Email is already in use")
                : ("Tên đăng nhập đã được sử dụng", "Username is already taken");

            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    En = messageEn,
                    Vi = message
                },
                Type = "DuplicateEntry",
                Code = 400,
                Field = field
            };

            return FailResponse(
                controller,
                400,
                message,
                messageEn,
                new List<Violation> { violation }
            );
        }

        public static ObjectResult InvalidOtpError(ControllerBase controller)
        {
            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    En = "Invalid or expired OTP",
                    Vi = "Mã OTP không hợp lệ hoặc đã hết hạn"
                },
                Type = "InvalidOtp",
                Code = 400
            };

            return FailResponse(
                controller,
                400,
                "Mã OTP không hợp lệ hoặc đã hết hạn",
                "Invalid or expired OTP",
                new List<Violation> { violation }
            );
        }

        public static ObjectResult NotFoundError(
            ControllerBase controller,
            string resourceName,
            string? messageEn = null)
        {
            var message = $"{resourceName} không tồn tại";
            messageEn ??= $"{resourceName} not found";

            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    En = messageEn,
                    Vi = message
                },
                Type = "NotFound",
                Code = 404
            };

            return FailResponse(
                controller,
                404,
                message,
                messageEn,
                new List<Violation> { violation }
            );
        }

        public static ObjectResult ValidationError(
            ControllerBase controller,
            List<Violation> violations)
        {
            return FailResponse(
                controller,
                400,
                "Dữ liệu không hợp lệ",
                "Validation failed",
                violations
            );
        }
    }
}

