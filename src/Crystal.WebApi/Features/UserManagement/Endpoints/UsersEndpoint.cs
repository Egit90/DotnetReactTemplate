using System.Text;
using Crystal.Core.Options;
using Crystal.Core.Services.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Extensions;
using WebApi.Features.UserManagement.Models;

namespace WebApi.Features.UserManagement.Endpoints;

public static class UsersEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        // List users
        group.MapGet("/users", GetAllUsers);

        // Delete user
        group.MapDelete("/users/{userId}", DeleteUser);

        // Update user roles
        group.MapPut("/users/{userId}/roles", UpdateUserRoles);

        // Resend email confirmation
        group.MapPost("/users/{userId}/resend-confirmation", ResendEmailConfirmation);

        // Get All Roles (Amin Only)
        group.MapGet("/roles", GetAllRoles);

        // Get system stats (Admin only)
        group.MapGet("/stats", GetStats);
    }

    // GET /api/admin/users
    private static async Task<IResult> GetAllUsers(UserManager<MyUser> userManager, ILogger<Program> logger, int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            var query = userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.UserName!.ToLower().Contains(lowerSearchTerm) ||
                    u.Email!.ToLower().Contains(lowerSearchTerm));
            }

            var usersPagedResult = await query.ToPagedResult(page, pageSize, logger);

            if (usersPagedResult.IsFailure)
            {
                return Results.Problem(
                    title: "Error fetching users",
                    detail: usersPagedResult.Error,
                    statusCode: 500
                );
            }

            var usersWithRoles = new List<object>();

            foreach (var user in usersPagedResult.Value.Items)
            {
                var roles = await userManager.GetRolesAsync(user);
                usersWithRoles.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.EmailConfirmed,
                    user.LastLoginDate,
                    user.CreatedOn,
                    user.LockoutEnd,
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = roles
                });
            }

            return Results.Ok(new
            {
                Data = usersWithRoles,
                Page = usersPagedResult.Value.CurrentPage,
                usersPagedResult.Value.PageSize,
                usersPagedResult.Value.TotalCount,
                usersPagedResult.Value.TotalPages,
                HasNextPage = usersPagedResult.Value.CurrentPage < usersPagedResult.Value.TotalPages,
                HasPreviousPage = usersPagedResult.Value.CurrentPage > 1
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error occurred while fetching users. Page: {Page}, PageSize: {PageSize}",
                page, pageSize);

            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }

    // DELETE /api/admin/users/{userId}
    private static async Task<IResult> DeleteUser(string userId, UserManager<MyUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Results.NotFound(new { Error = "User not found" });

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return Results.BadRequest(new { Errors = result.Errors });

        return Results.Ok(new { Message = "User deleted successfully" });
    }

    // PUT /api/admin/users/{userId}/roles
    private static async Task<IResult> UpdateUserRoles(string userId, UpdateUserRolesRequest request, UserManager<MyUser> userManager, ILogger<Program> logger)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound(new { Error = "User not found" });

            var currentRoles = await userManager.GetRolesAsync(user);

            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return Results.BadRequest(new { removeResult.Errors });

            if (request.RoleNames?.Length > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, request.RoleNames);
                if (!addResult.Succeeded)
                    return Results.BadRequest(new { Errors = addResult.Errors });
            }

            return Results.Ok(new { Message = "User roles updated successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user roles. UserId: {UserId}", userId);
            return Results.Problem("Unexpected error occurred while updating user roles.");
        }
    }

    // POST /api/admin/users/{userId}/resend-confirmation
    private static async Task<IResult> ResendEmailConfirmation(string userId, UserManager<MyUser> userManager, ICrystalEmailSenderManager<MyUser, Guid> emailSender, IOptions<CrystalOptions> options,
        ILogger<Program> logger,
        HttpRequest httpRequest)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound(new { Error = "User not found" });

            if (user.EmailConfirmed) return Results.BadRequest(new { Error = "Email already confirmed" });

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var baseUri = new Uri(
                new Uri(options.Value.ClientApp.BaseUrl ?? $"{httpRequest.Scheme}://{httpRequest.Host}"),
                options.Value.ClientApp.EmailConfirmationPath);

            var link = new Uri(baseUri, $"?code={code}&userId={user.Id}");

            await emailSender.SendEmailConfirmationAsync(user, link.ToString());

            return Results.Ok(new { Message = "Confirmation email sent successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resending confirmation email. UserId: {UserId}", userId);
            return Results.Problem("Unexpected error occurred while resending confirmation email.");
        }

    }

    private static async Task<IResult> GetAllRoles(RoleManager<IdentityRole<Guid>> roleManager, ILogger<Program> logger)
    {
        try
        {
            var roles = await roleManager.Roles
                .Select(r => new { r.Id, r.Name })
                .ToListAsync();

            return Results.Ok(roles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while getting all roles");
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }

    private static IResult GetStats(UserManager<MyUser> userManager)
    {
        var totalUsers = userManager.Users.Count();
        var confirmedUsers = userManager.Users.Count(u => u.EmailConfirmed);
        var lockedUsers = userManager.Users.Count(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow);

        // Active users: logged in within the last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var activeUsers = userManager.Users.Count(u => u.LastLoginDate.HasValue && u.LastLoginDate > thirtyDaysAgo);

        return Results.Ok(new
        {
            TotalUsers = totalUsers,
            ConfirmedUsers = confirmedUsers,
            UnconfirmedUsers = totalUsers - confirmedUsers,
            LockedUsers = lockedUsers,
            ActiveUsers = activeUsers
        });
    }

    private record UpdateUserRolesRequest(string[] RoleNames);
}
