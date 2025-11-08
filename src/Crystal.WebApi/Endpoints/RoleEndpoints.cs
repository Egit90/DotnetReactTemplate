using Microsoft.AspNetCore.Identity;
using WebApi.Data;

namespace WebApi.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles")
            .RequireAuthorization();

        // Get user's roles
        group.MapGet("/my-roles", GetMyRoles);
    }

    private static async Task<IResult> GetMyRoles(
        HttpContext context,
        UserManager<MyUser> userManager)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        if (userId == null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Results.NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Results.Ok(new { UserId = userId, Roles = roles });
    }
}
