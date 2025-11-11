using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi.Features.SystemLogs.Endpoints;

public static class SystemLogsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        // GET /api/admin/logs
        group.MapGet("/logs", GetLogs);
    }

    private static async Task<IResult> GetLogs(
        ApplicationDbContext dbContext,
        ILogger<Program> logger,
        int page = 1,
        int pageSize = 50,
        string? level = null,
        string? correlationId = null,
        string? requestPath = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = dbContext.Logs.AsQueryable();

            // Filter by level
            if (!string.IsNullOrWhiteSpace(level))
            {
                var levelInt = level.ToLower() switch
                {
                    "verbose" => 0,
                    "debug" => 1,
                    "information" => 2,
                    "warning" => 3,
                    "error" => 4,
                    "fatal" => 5,
                    _ => -1
                };

                if (levelInt >= 0)
                    query = query.Where(l => l.Level == levelInt);
            }

            // Filter by correlation ID (inside JSON)
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                query = query.Where(l =>
                    l.LogEvent != null &&
                    l.LogEvent.Properties!.CorrelationId != null &&
                    EF.Functions.ILike(l.LogEvent.Properties.CorrelationId, $"%{correlationId}%"));
            }

            // Filter by request path (inside JSON)
            if (!string.IsNullOrWhiteSpace(requestPath))
            {
                query = query.Where(l =>
                    l.LogEvent != null &&
                    l.LogEvent.Properties!.RequestPath != null &&
                    EF.Functions.ILike(l.LogEvent.Properties.RequestPath, $"%{requestPath}%"));
            }

            // Count for pagination
            var totalCount = await query.CountAsync();

            // Get paged data
            var logs = await query
                        .OrderByDescending(l => l.Timestamp)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(l => new
                        {
                            l.Timestamp,
                            l.Level,
                            l.Message,
                            l.Exception,
                            CorrelationId = l.LogEvent!.Properties!.CorrelationId,
                            RequestPath = l.LogEvent!.Properties!.RequestPath,
                            RequestMethod = l.LogEvent!.Properties!.RequestMethod,
                            UserAgent = l.LogEvent!.Properties!.UserAgent,
                            RemoteIpAddress = l.LogEvent!.Properties!.RemoteIpAddress,
                            Application = l.LogEvent!.Properties!.Application,
                            ContextType = l.LogEvent!.Properties!.ContextType,
                            Error = l.LogEvent!.Properties!.Error,
                            SourceContext = l.LogEvent!.Properties!.SourceContext
                        })
                        .ToListAsync();

            // Convert numeric levels to names and add sequential index
            var data = logs.Select((l, index) => new
            {
                Id = (page - 1) * pageSize + index + 1,
                l.Timestamp,
                Level = l.Level switch
                {
                    0 => "Verbose",
                    1 => "Debug",
                    2 => "Information",
                    3 => "Warning",
                    4 => "Error",
                    5 => "Fatal",
                    _ => "Unknown"
                },
                l.Message,
                l.Exception,
                l.CorrelationId,
                l.RequestPath,
                l.RequestMethod,
                l.UserAgent,
                l.RemoteIpAddress,
                l.Application,
                l.ContextType,
                l.Error,
                l.SourceContext
            }).ToList();

            // Return paged response
            return Results.Ok(new
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasNextPage = page * pageSize < totalCount,
                HasPreviousPage = page > 1
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching logs. Page: {Page}, Size: {Size}", page, pageSize);
            return Results.Problem("Unexpected error occurred while fetching logs.");
        }
    }
}
