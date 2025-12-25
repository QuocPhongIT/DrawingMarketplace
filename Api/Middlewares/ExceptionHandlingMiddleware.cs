using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using DrawingMarketplace.Domain.Exceptions;

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
                Console.WriteLine(
                    $"🔥 DOMAIN EX: {ex.GetType().Name} | Status={ex.StatusCode}");
                await HandleDomainException(context, ex);
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
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var problem = new ValidationProblemDetails(errors)
            {
                Type = GetRfcType(StatusCodes.Status400BadRequest),
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }

        private static async Task HandleDomainException(
            HttpContext context,
            DomainException ex)
        {
            var problem = new ProblemDetails
            {
                Type = GetRfcType(ex.StatusCode),
                Title = "Business Error",
                Status = ex.StatusCode,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
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

            var rootMessage = ex.InnerException?.Message ?? ex.Message;

            var problem = new ProblemDetails
            {
                Type = GetRfcType(StatusCodes.Status500InternalServerError),
                Title = "Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _env.IsDevelopment()
                    ? rootMessage
                    : "An unexpected error occurred.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }


        private static string GetRfcType(int statusCode) => statusCode switch
        {
            400 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            401 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.2",
            403 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            404 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            409 => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };
    }
}
