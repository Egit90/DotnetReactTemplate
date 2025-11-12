using System.Security.Claims;
using Crystal.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Crystal.Core.Endpoints.SignIn;

public class SignInRefreshEndpoint<TUser, TKey> : IAuthEndpoint
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        , ICrystalUser<TKey>
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost(
                "/signin/refresh",
                async Task<Results<SignInHttpResult, UnauthorizedHttpResult, EmptyHttpResult>> (
                    [FromQuery] bool? useCookie,
                    [FromServices] UserManager<TUser> manager,
                    [FromServices] IRefreshTokenManager<TKey> refreshTokenManager,
                    [FromServices] CrystalSignInManager<TUser, TKey> signInManager,
                    HttpContext context) =>
                {
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var token = context.User.FindFirstValue(CrystalClaimTypes.RefreshToken);

                    if (userId == null || token == null)
                    {
                        return TypedResults.Unauthorized();
                    }

                    var user = await manager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return TypedResults.Unauthorized();
                    }

                    if (!await refreshTokenManager.ValidateAsync(user.Id, token))
                    {
                        return TypedResults.Unauthorized();
                    }

                    signInManager.UseCookie = useCookie ?? false;
                    await signInManager.SignInAsync(user, new AuthenticationProperties(), context.User.Identity.AuthenticationType);

                    return TypedResults.Empty;
                })
            .RequireAuthorization(b =>
            {
                b.RequireAuthenticatedUser();
                b.AddAuthenticationSchemes(CrystalAuthSchemeDefaults.RefreshTokenScheme);
            });
    }
}