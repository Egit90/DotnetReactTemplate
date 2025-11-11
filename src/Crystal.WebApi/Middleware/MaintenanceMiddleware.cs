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
                !path.StartsWith("/api/login") &&
                !path.StartsWith("/api/signin"))
            {
                _logger.LogInformation("Blocked {Path} during maintenance", path);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "System is under maintenance. Please try again later.",
                    maintenanceMode = true
                });
                return;
            }
        }

        await _next(context);
    }
}
