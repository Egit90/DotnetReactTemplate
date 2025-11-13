using Crystal.Core.Abstractions;
using Crystal.Core.Services.EmailSender;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Crystal.Core.Endpoints;

public class WhoAmIEndpoint<TUser, TKey> : IAuthEndpoint
        where TKey : IEquatable<TKey>
        where TUser : ICrystalUser<TKey>
{
    public RouteHandlerBuilder Map(IEndpointRouteBuilder builder)
    {
        return builder.MapGet("/whoami", (HttpContext context) =>
        {
            var response = new WhoAmIResponse
            {
                Username = context.User.Identity?.Name,
                Email = context.User.FindFirstValue(ClaimTypes.Email),
                Roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
            };

            return TypedResults.Ok(response);
        }).RequireAuthorization();
    }
}

public class WhoAmIResponse
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string> Roles { get; set; } = [];
}