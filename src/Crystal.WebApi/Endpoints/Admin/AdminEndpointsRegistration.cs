using WebApi.Endpoints.Admin.Lock;
using WebApi.Endpoints.Admin.Logs;
using WebApi.Endpoints.Admin.Users;

namespace WebApi.Endpoints.Admin;

public static class AdminEndpointsRegistration
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));


        LogsEndpoint.Map(group);
        UsersEndpoints.Map(group);
        AccountLockEndpoint.Map(group);
    }
}
