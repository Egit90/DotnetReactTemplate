using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Crystal.Core.Endpoints.External;

public class ExternalProvidersEndpoint<TUser, TKey> : IAuthEndpoint
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        , ICrystalUser<TKey>
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/external/providers", async ([FromServices] SignInManager<TUser> signInManager) =>
        {
            var schemes = (await signInManager.GetExternalAuthenticationSchemesAsync())
                .Select(s => s.Name)
                .ToList();
            var res = new ExternalProvidersResponse
            {
                Providers = schemes
            };

            return TypedResults.Ok(res);
        }).AllowAnonymous();
    }
}
