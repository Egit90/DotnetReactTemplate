using Crystal.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Crystal.Core.Endpoints.SignOut;

public class SignOutEndpoint<TUser, TKey> : IAuthEndpoint
        where TKey : IEquatable<TKey>
        where TUser : ICrystalUser<TKey>
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/signout", async Task<Ok>
            (HttpContext context) =>
            {
                await context.SignOutAsync(CrystalAuthSchemeDefaults.BearerSignInScheme);
                return TypedResults.Ok();
            })
            .RequireAuthorization();
    }
}