using Crystal.Core.Abstractions;
using Crystal.Core.Services.EmailSender;
﻿using System.Security.Claims;
using Crystal.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Crystal.Core.Endpoints;

public class SignOutEndpoint<TUser> : IAuthEndpoint where TUser : ICrystalUser
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