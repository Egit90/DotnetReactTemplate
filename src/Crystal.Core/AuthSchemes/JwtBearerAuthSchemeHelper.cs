using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Crystal.Core.AuthSchemes;

public static class JwtBearerAuthSchemeHelper
{
    public static void ConfigureBearerAuth(
        this JwtBearerOptions bearerOptions, CrystalJwtBearerOptions jwtBearerOptions)
    {
        if (jwtBearerOptions.SigningKey is null)
        {
            throw new ArgumentNullException(nameof(jwtBearerOptions.SigningKey));
        }

        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtBearerOptions.SigningKey));
        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = jwtBearerOptions.Audience,
            ValidIssuer = jwtBearerOptions.Issuer,
        };

        bearerOptions.Events ??= new JwtBearerEvents();
        bearerOptions.Events.OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers["X-Token-Expired"] = "true";
            }

            return Task.CompletedTask;
        };

        bearerOptions.TokenValidationParameters.ValidateAudience =
            bearerOptions.TokenValidationParameters.ValidAudience is not null;
        bearerOptions.TokenValidationParameters.ValidateIssuer =
            bearerOptions.TokenValidationParameters.ValidIssuer is not null;
    }
}