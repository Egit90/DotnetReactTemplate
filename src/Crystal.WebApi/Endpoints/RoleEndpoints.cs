using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data;

namespace WebApi.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
            .WithTags("Roles")
            .RequireAuthorization();

        // Get all roles (Admin only)
        group.MapGet("/", GetAllRoles)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Get user's roles
        group.MapGet("/my-roles", GetMyRoles);

        // Assign role to user (Admin only)
        group.MapPost("/assign", AssignRole)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Remove role from user (Admin only)
        group.MapPost("/remove", RemoveRole)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }

    private static async Task<IResult> GetAllRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
        return Results.Ok(roles);
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

    private static async Task<IResult> AssignRole(
        [FromBody] AssignRoleRequest request,
        UserManager<MyUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound(new { Error = "User not found" });

        if (!await roleManager.RoleExistsAsync(request.RoleName))
            return Results.BadRequest(new { Error = "Role does not exist" });

        var result = await userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
            return Results.BadRequest(new { Errors = result.Errors });

        return Results.Ok(new { Message = $"Role '{request.RoleName}' assigned to user successfully" });
    }

    private static async Task<IResult> RemoveRole(
        [FromBody] AssignRoleRequest request,
        UserManager<MyUser> userManager)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Results.NotFound(new { Error = "User not found" });

        var result = await userManager.RemoveFromRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
            return Results.BadRequest(new { Errors = result.Errors });

        return Results.Ok(new { Message = $"Role '{request.RoleName}' removed from user successfully" });
    }
}

public record AssignRoleRequest(string UserId, string RoleName);
