using DrawingMarketplace.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DrawingMarketplace.Api.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static ObjectResult Success<T>(
            this ControllerBase controller,
            T? data = default,
            string message = "Thành công",
            string? messageEn = null,
            int statusCode = 200)
        {
            return ResponseHelper.SuccessResponse(controller, statusCode, message, data, messageEn);
        }

        public static ObjectResult Error(
            this ControllerBase controller,
            string message,
            string? messageEn = null,
            int statusCode = 500,
            List<Violation>? violations = null)
        {
            return ResponseHelper.ErrorResponse(controller, statusCode, message, messageEn, violations);
        }

        public static ObjectResult Fail(
            this ControllerBase controller,
            string message,
            string? messageEn = null,
            int statusCode = 400,
            List<Violation>? violations = null)
        {
            return ResponseHelper.FailResponse(controller, statusCode, message, messageEn, violations);
        }

        public static ObjectResult NotFound(
            this ControllerBase controller,
            string resourceName = "Resource",
            string? messageEn = null)
        {
            return ResponseHelper.NotFoundError(controller, resourceName, messageEn);
        }

        public static ObjectResult InvalidCredentials(this ControllerBase controller)
        {
            return ResponseHelper.InvalidCredentialsError(controller);
        }

        public static ObjectResult AccountNotActive(this ControllerBase controller)
        {
            return ResponseHelper.AccountNotActiveError(controller);
        }

        public static ObjectResult DuplicateEntry(this ControllerBase controller, string field)
        {
            return ResponseHelper.DuplicateEntryError(controller, field);
        }

        public static ObjectResult InvalidOtp(this ControllerBase controller)
        {
            return ResponseHelper.InvalidOtpError(controller);
        }

        public static ObjectResult ValidationFailed(
            this ControllerBase controller,
            List<Violation> violations)
        {
            return ResponseHelper.ValidationError(controller, violations);
        }
    }
}

