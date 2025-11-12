using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Crystal.Core.Endpoints.Password;

public class PasswordChangeEndpoint<TUser> : IAccountEndpoint where TUser : IdentityUser<Guid>, ICrystalUser
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapPost("/password/change",
                async Task<Results<EmptyHttpResult, ProblemHttpResult>>
                ([FromBody, Required] ChangePasswordRequest req,
                    [FromServices] UserManager<TUser> manager,
                    HttpContext context,
                    [FromServices] ILogger<PasswordChangeEndpoint<TUser>> logger) =>
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.Password);
                    ArgumentException.ThrowIfNullOrWhiteSpace(req.NewPassword);

                    var user = await manager.GetUserAsync(context.User);
                    if (user == null)
                    {
                        logger.LogInformation("User not found");
                        return TypedResults.Problem(statusCode: 404);
                    }

                    var result = await manager.ChangePasswordAsync(user, req.Password, req.NewPassword);
                    if (!result.Succeeded)
                    {
                        logger.LogInformation(
                            "User {UserId} failed to change password. Result: {Result}", user.Id, result);
                        return TypedResults.Problem(result.ToValidationProblem());
                    }

                    logger.LogInformation("User {UserId} changed password successfully", user.Id);
                    return TypedResults.Empty;
                })
            .AddEndpointFilter<ValidationEndpointFilter<ChangePasswordRequest>>()
            .RequireAuthorization();
    }
}
