using WebApi.Features.AccountLocking.Endpoints;
using WebApi.Features.SystemLogs.Endpoints;
using WebApi.Features.UserManagement.Endpoints;

namespace WebApi.Features;

public static class FeaturesRegistration
{
    public static void MapFeaturesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));


        SystemLogsEndpoint.Map(group);
        UsersEndpoints.Map(group);
        AccountLockEndpoint.Map(group);
    }
}
