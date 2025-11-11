using WebApi.Services;

namespace WebApi.Middleware;

public class MaintenanceMiddleware(RequestDelegate _next, IMaintenanceService _service, ILogger<MaintenanceMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (_service.IsEnabled)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Allow admin endpoints, health checks, and login so admins can disable maintenance
            if (!path.StartsWith("/api/admin") &&
                !path.StartsWith("/health") &&
                !path.StartsWith("/api/auth/"))
            {
                _logger.LogInformation("Blocked {Path} during maintenance", path);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                    title = "Service Unavailable",
                    status = 503,
                    detail = "System is under maintenance. Please try again later.",
                    maintenanceMode = true
                });
                return;
            }
        }

        await _next(context);
    }
}
