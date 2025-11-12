using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crystal.Core.AuthSchemes;

public class CrystalSignInJwtBearerHandler<TKey>(
    IOptionsMonitor<CrystalJwtBearerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IJwtTokenService<TKey> tokenService,
    IRefreshTokenManager<TKey> refreshTokenManager)
    : SignInAuthenticationHandler<CrystalJwtBearerOptions>(options, logger, encoder)
    where TKey : IEquatable<TKey>
{
    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var (token, expiresAt) = tokenService.CreateAccessToken(user);
        var refreshToken = await refreshTokenManager.CreateTokenAsync(user);
        var (refreshJwtToken, refreshExpiresAt) = tokenService.CreateBearerRefreshToken(refreshToken);

        Context.Response.Cookies.Append(
            CrystalAuthSchemeDefaults.RefreshTokenCookieName,
            refreshJwtToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshExpiresAt,
            });

        var useCookie = properties?.GetParameter<bool?>("useCookie") ?? false;
        if (useCookie)
        {
            Context.Response.Cookies.Append(
                CrystalAuthSchemeDefaults.AccessTokenCookieName,
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = expiresAt,
                });
        }

        var accessTokenResponse = new AccessTokenResponse
        {
            AccessToken = useCookie ? null : token,
            ExpiresIn = (long)(expiresAt - DateTime.UtcNow).TotalSeconds,
        };

        await Context.Response.WriteAsJsonAsync(
            accessTokenResponse, AccessTokenResponseJsonSerializerContext.Default.AccessTokenResponse);
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        var userIdString = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString != null)
        {
            TKey userId = typeof(TKey) == typeof(Guid)
                        ? (TKey)(object)Guid.Parse(userIdString)
                        : (TKey)Convert.ChangeType(userIdString, typeof(TKey));

            refreshTokenManager.ClearTokenAsync(userId);
        }
        Context.Response.Cookies.Delete(CrystalAuthSchemeDefaults.RefreshTokenCookieName);
        return Task.CompletedTask;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        throw new NotSupportedException();
    }
}