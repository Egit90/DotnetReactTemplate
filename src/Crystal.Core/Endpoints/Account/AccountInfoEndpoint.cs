using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Crystal.Core.Endpoints.Account;

public class AccountInfoEndpoint<TUser, TKey> : IAccountEndpoint
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        , ICrystalUser<TKey>
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/info", async Task<Results<Ok<AccountInfoResponse>, NotFound>> (
            HttpContext context,
            ILogger<AccountInfoEndpoint<TUser, TKey>> logger,
            UserManager<TUser> userManager) =>
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId is not null ? await userManager.FindByIdAsync(userId) : null;
            if (user is null)
            {
                logger.LogError("User: {UserId} not found, but it is authenticated", userId);
                return TypedResults.NotFound();
            }

            var logins = await userManager.GetLoginsAsync(user);
            var roles = await userManager.GetRolesAsync(user);

            var res = new AccountInfoResponse
            {
                Email = user.Email,
                Username = user.UserName,
                HasPassword = await userManager.HasPasswordAsync(user),
                Roles = roles.ToList(),
                Logins = logins.Select(l => l.LoginProvider).ToList()
            };

            return TypedResults.Ok(res);
        });
    }
}
