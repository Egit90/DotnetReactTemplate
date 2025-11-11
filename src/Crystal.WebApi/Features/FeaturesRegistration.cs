using WebApi.Features.Lock.Endpoints;
using WebApi.Features.Logs.Endpoints;
using WebApi.Features.UserManagement.Endpoints;

namespace WebApi.Features;

public static class FeaturesRegistration
{
    public static void MapFeaturesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));


        LogsEndpoint.Map(group);
        UsersEndpoints.Map(group);
        AccountLockEndpoint.Map(group);
    }
}
