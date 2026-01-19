using FluentValidation;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Api.Responses;

namespace DrawingMarketplace.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            IWebHostEnvironment env,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationException(context, ex);
            }
            catch (DomainException ex)
            {
                await HandleDomainException(context, ex);
            }
            catch (FormatException ex)
            {
                await HandleFormatException(context, ex);
            }
            catch (Exception ex)
            {
                await HandleUnhandledException(context, ex);
            }
        }

        private static async Task HandleValidationException(
            HttpContext context,
            ValidationException ex)
        {
            var violations = ex.Errors
                .GroupBy(e => e.PropertyName)
                .SelectMany(g => g.Select(e => new Violation
                {
                    Message = new ViolationMessage
                    {
                        Vi = e.ErrorMessage,
                        En = e.ErrorMessage
                    },
                    Type = "ValidationError",
                    Code = 400,
                    Field = g.Key
                }))
                .ToList();

            await ResponseHelper.WriteErrorResponseAsync(
                context,
                400,
                "Dữ liệu không hợp lệ",
                "Validation failed",
                violations,
                "fail");
        }

        private static async Task HandleDomainException(
            HttpContext context,
            DomainException ex)
        {
            string messageVi;
            string messageEn;

            if (ex is NotFoundException notFoundEx)
            {
                var resourceNameVi = GetResourceNameVietnamese(notFoundEx.ResourceName);
                messageVi = $"{resourceNameVi} với mã '{notFoundEx.Key}' không tồn tại.";
                messageEn = ex.Message;
            }
            else
            {
                messageVi = ex.Message;
                messageEn = ex.Message;
            }

            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    Vi = messageVi,
                    En = messageEn
                },
                Type = ex.GetType().Name.Replace("Exception", ""),
                Code = ex.StatusCode
            };

            await ResponseHelper.WriteErrorResponseAsync(
                context,
                ex.StatusCode,
                messageVi,
                messageEn,
                new List<Violation> { violation },
                "fail");
        }

        private static string GetResourceNameVietnamese(string resourceName)
        {
            return resourceName.ToLower() switch
            {
                "content" => "Nội dung",
                "cart" => "Giỏ hàng",
                "order" => "Đơn hàng",
                "user" => "Người dùng",
                "category" => "Danh mục",
                "coupon" => "Mã giảm giá",
                "collaborator" => "Cộng tác viên",
                "collaboratorrequest" => "Yêu cầu cộng tác viên",
                "wallet" => "Ví",
                "withdrawal" => "Yêu cầu rút tiền",
                "otp" => "Mã OTP",
                "review" => "Đánh giá",
                "banner" => "Banner",
                "copyrightreport" => "Báo cáo vi phạm",
                "mediafile" => "File",
                "bankaccount" => "Tài khoản ngân hàng",
                _ => resourceName
            };
        }

        private static async Task HandleFormatException(
            HttpContext context,
            FormatException ex)
        {
            var violation = new Violation
            {
                Message = new ViolationMessage
                {
                    Vi = "Định dạng dữ liệu không hợp lệ",
                    En = "Invalid data format"
                },
                Type = "FormatError",
                Code = 400
            };

            await ResponseHelper.WriteErrorResponseAsync(
                context,
                400,
                "Định dạng dữ liệu không hợp lệ",
                "Invalid data format",
                new List<Violation> { violation },
                "fail");
        }

        private async Task HandleUnhandledException(
            HttpContext context,
            Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception | TraceId: {TraceId} | Path: {Path}",
                context.TraceIdentifier,
                context.Request.Path
            );

            var message = _env.IsDevelopment()
                ? (ex.InnerException?.Message ?? ex.Message)
                : "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.";

            var messageEn = _env.IsDevelopment()
                ? (ex.InnerException?.Message ?? ex.Message)
                : "An unexpected error occurred. Please try again later.";

            await ResponseHelper.WriteErrorResponseAsync(
                context,
                500,
                message,
                messageEn,
                null,
                "error");
        }
    }
}
