using Microsoft.AspNetCore.Identity;
using WebApi.Data;

namespace WebApi.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Get all users (Admin only)
        group.MapGet("/users", GetAllUsers);

        // Delete user (Admin only)
        group.MapDelete("/users/{userId}", DeleteUser);

        // Get system stats (Admin only)
        group.MapGet("/stats", GetStats);
    }

    private static async Task<IResult> GetAllUsers(UserManager<MyUser> userManager)
    {
        var users = userManager.Users
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.EmailConfirmed,
                u.AboutMe,
                u.MySiteUrl
            })
            .ToList();

        return Results.Ok(users);
    }

    private static async Task<IResult> DeleteUser(
        string userId,
        UserManager<MyUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Results.NotFound(new { Error = "User not found" });

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return Results.BadRequest(new { Errors = result.Errors });

        return Results.Ok(new { Message = "User deleted successfully" });
    }

    private static IResult GetStats(UserManager<MyUser> userManager)
    {
        var totalUsers = userManager.Users.Count();
        var confirmedUsers = userManager.Users.Count(u => u.EmailConfirmed);

        return Results.Ok(new
        {
            TotalUsers = totalUsers,
            ConfirmedUsers = confirmedUsers,
            UnconfirmedUsers = totalUsers - confirmedUsers
        });
    }
}
