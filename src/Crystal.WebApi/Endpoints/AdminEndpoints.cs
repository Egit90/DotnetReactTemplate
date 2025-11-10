using System.Text;
using Crystal.Core.Options;
using Crystal.Core.Services.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Data;
using WebApi.Extensions;

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

        // Get All Roles (Amin Only)
        group.MapGet("/roles", GetAllRoles);

        // Update user roles (Admin only)
        group.MapPut("/users/{userId}/roles", UpdateUserRoles);

        // Get system stats (Admin only)
        group.MapGet("/stats", GetStats);

        // Resend Email Confirmation (Admin only)
        group.MapPost("/users/{userId}/resend-confirmation", ResendEmailConfirmation);

        // lock user account
        group.MapPost("/user/{userId}/lock", LockUser);

        // Unlock user account
        group.MapPost("/user/{userId}/unLock", UnlockUser);

        // Get logs (Admin only)
        group.MapGet("/logs", GetLogs);
    }

    private static async Task<IResult> GetLogs(
        ApplicationDbContext dbContext,
        ILogger<Program> logger,
        int page = 1,
        int pageSize = 50,
        string? level = null)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            // Query logs from the logs table created by Serilog.Sinks.PostgreSQL
            // Note: Serilog stores log levels as integers: Verbose=0, Debug=1, Information=2, Warning=3, Error=4, Fatal=5

            // Map level name to integer if filtering
            int? levelInt = null;
            if (!string.IsNullOrEmpty(level))
            {
                levelInt = level switch
                {
                    "Verbose" => 0,
                    "Debug" => 1,
                    "Information" => 2,
                    "Warning" => 3,
                    "Error" => 4,
                    "Fatal" => 5,
                    _ => -1
                };
            }

            // Get total count
            var countQuery = levelInt.HasValue
                ? dbContext.Database.SqlQuery<int>($"SELECT COUNT(*) as Value FROM logs WHERE level = {levelInt.Value}")
                : dbContext.Database.SqlQuery<int>($"SELECT COUNT(*) as Value FROM logs");

            var totalCount = await countQuery.FirstOrDefaultAsync();

            // Get paginated logs
            var offset = (page - 1) * pageSize;
            List<LogEntry> logs;

            if (levelInt.HasValue)
            {
                logs = await dbContext.Database
                    .SqlQuery<LogEntry>($"""
                        SELECT
                            ROW_NUMBER() OVER (ORDER BY timestamp DESC) as Id,
                            timestamp as Timestamp,
                            CASE level
                                WHEN 0 THEN 'Verbose'
                                WHEN 1 THEN 'Debug'
                                WHEN 2 THEN 'Information'
                                WHEN 3 THEN 'Warning'
                                WHEN 4 THEN 'Error'
                                WHEN 5 THEN 'Fatal'
                                ELSE 'Unknown'
                            END as Level,
                            message as Message,
                            exception as Exception,
                            log_event::text as Properties
                        FROM logs
                        WHERE level = {levelInt.Value}
                        ORDER BY timestamp DESC
                        LIMIT {pageSize} OFFSET {offset}
                        """)
                    .ToListAsync();
            }
            else
            {
                logs = await dbContext.Database
                    .SqlQuery<LogEntry>($"""
                        SELECT
                            ROW_NUMBER() OVER (ORDER BY timestamp DESC) as Id,
                            timestamp as Timestamp,
                            CASE level
                                WHEN 0 THEN 'Verbose'
                                WHEN 1 THEN 'Debug'
                                WHEN 2 THEN 'Information'
                                WHEN 3 THEN 'Warning'
                                WHEN 4 THEN 'Error'
                                WHEN 5 THEN 'Fatal'
                                ELSE 'Unknown'
                            END as Level,
                            message as Message,
                            exception as Exception,
                            log_event::text as Properties
                        FROM logs
                        ORDER BY timestamp DESC
                        LIMIT {pageSize} OFFSET {offset}
                        """)
                    .ToListAsync();
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Results.Ok(new
            {
                Data = logs,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while fetching logs. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }

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

    private static async Task<IResult> UpdateUserRoles(
        string userId,
        UpdateUserRolesRequest request,
        UserManager<MyUser> userManager,
        ILogger<Program> logger)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound(new { Error = "User not found" });

            // Get current roles
            var currentRoles = await userManager.GetRolesAsync(user);

            // Remove all current roles
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return Results.BadRequest(new { removeResult.Errors });

            // Add new roles
            if (request.RoleNames != null && request.RoleNames.Length > 0)
            {
                var addResult = await userManager.AddToRolesAsync(user, request.RoleNames);
                if (!addResult.Succeeded)
                {
                    return Results.BadRequest(new { Errors = addResult.Errors });
                }
            }

            return Results.Ok(new { Message = "User roles updated successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while updating user roles. UserId: {UserId}", userId);
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }

    private static async Task<IResult> GetAllRoles(RoleManager<IdentityRole> roleManager, ILogger<Program> logger)
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

    private static async Task<IResult> GetAllUsers(
        UserManager<MyUser> userManager,
        ILogger<Program> logger,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null)
    {
        try
        {
            var query = userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(u => u.UserName!.ToLower().Contains(lowerSearchTerm)
                                        || u.Email!.ToLower().Contains(lowerSearchTerm));
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

            List<object> usersWithRoles = [];

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
            logger.LogError(ex, "Unexpected error occurred while fetching users. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
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

    private static async Task<IResult> ResendEmailConfirmation(
        string userId,
        UserManager<MyUser> userManager,
        ICrystalEmailSenderManager<MyUser> emailSender,
        IOptions<CrystalOptions> options,
        ILogger<Program> logger,
        HttpRequest httpRequest)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Results.NotFound(new { Error = "User not found" });

            if (user.EmailConfirmed) return Results.BadRequest(new { Error = "Email already confirmed" });

            // Generate confirmation token
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Build confirmation link
            var baseUri = new Uri(
                        new Uri(options.Value.ClientApp.BaseUrl ?? $"{httpRequest.Scheme}://{httpRequest.Host}"),
                        options.Value.ClientApp.EmailConfirmationPath);
            var link = new Uri(baseUri, $"?code={code}&userId={user.Id}");

            // Send email
            await emailSender.SendEmailConfirmationAsync(user, link.ToString());

            return Results.Ok(new { Message = "Confirmation email sent successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while resending confirmation email. UserId: {UserId}", userId);
            return Results.Problem(
                title: "Unexpected error",
                detail: "An unexpected error occurred. Please try again later.",
                statusCode: 500
            );
        }
    }
}

internal record UpdateUserRolesRequest(string[] RoleNames);

internal class LogEntry
{
    public int Id { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Level { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? Properties { get; set; }
}