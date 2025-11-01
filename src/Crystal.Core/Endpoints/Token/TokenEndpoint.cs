using System.ComponentModel.DataAnnotations;
using Crystal.Core.AuthSchemes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Crystal.Core.Endpoints.Token;

public class TokenEndpoint<TUser> : IAuthEndpoint where TUser : class, ICrystalUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/token", async Task<Results<SignInHttpResult, ProblemHttpResult, EmptyHttpResult>>
            ([FromBody, Required] TokenRequest req,
                [FromServices] SignInManager<TUser> manager,
                [FromServices] ILogger<TokenEndpoint<TUser>> logger) =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Email);
                ArgumentException.ThrowIfNullOrWhiteSpace(req.Password);

                manager.AuthenticationScheme = CrystalAuthSchemeDefaults.BearerTokenScheme;

                var result =
                    await manager.PasswordSignInAsync(req.Email, req.Password, isPersistent: false, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    logger.LogInformation("User {Email} failed to sign in. Result: {Result}", req.Email, result);
                    return TypedResults.Problem(
                        "Invalid email or password",
                        statusCode: StatusCodes.Status401Unauthorized);
                }

                return TypedResults.Empty;
            })
            .AddEndpointFilter<ValidationEndpointFilter<TokenRequest>>()
            .AllowAnonymous();
    }
}
