using System.Net;
using System.Text.Json;
using FleetTelemetryPlatform.Services;

namespace FleetTelemetryPlatform.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                DeviceNotFoundException => (HttpStatusCode.NotFound, exception.Message),
                DeviceConcurrencyException => (HttpStatusCode.Conflict, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception occurred");
            else
                _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { error = message };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}