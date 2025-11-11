using Microsoft.AspNetCore.Identity;
using WebApi.Features.UserManagement.Models;

namespace WebApi.Features.Lock.Endpoints;

public static class AccountLockEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        // lock user account
        group.MapPost("/user/{userId}/lock", LockUser);

        // Unlock user account
        group.MapPost("/user/{userId}/unLock", UnlockUser);
    }
    // POST /api/admin/user/{userId}/lock
    private static async Task<IResult> LockUser(string userId, UserManager<MyUser> userManager, ILogger<Program> logger)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound(new { Error = "User was not found" });

            // Set lockout end date to far future (permanent lock)
            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!result.Succeeded) return Results.BadRequest(new { result.Errors });

            // Also ensure lockout is enabled for this user
            await userManager.SetLockoutEnabledAsync(user, true);

            logger.LogInformation("User {userEmail} was locked", user.Email);
            return Results.Ok(new { Message = $"User was locked" });

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error locking user. UserId: {UserId}", userId);
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }

    // POST /api/admin/user/{userId}/unlock
    private static async Task<IResult> UnlockUser(string userId, UserManager<MyUser> userManager, ILogger<Program> logger)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound(new { Error = "User was not found" });

            // Remove lockout by setting end date to null
            var result = await userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded) return Results.BadRequest(new { result.Errors });

            logger.LogInformation("User {userEmail} was unlocked", user.Email);
            return Results.Ok(new { Message = $"User was unlocked" });

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unlocking user. UserId: {UserId}", userId);
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }



}