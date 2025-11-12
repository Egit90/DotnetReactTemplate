using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crystal.Core.AuthSchemes;

public class CrystalTokenJwtBearerHandler<TKey>(
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
        var (refreshJwtToken, _) = tokenService.CreateBearerRefreshToken(refreshToken);

        var accessTokenResponse = new AccessTokenResponse
        {
            AccessToken = token,
            RefreshToken = refreshJwtToken,
            ExpiresIn = (long)(expiresAt - DateTime.UtcNow).TotalSeconds
        };

        await Context.Response.WriteAsJsonAsync(
            accessTokenResponse, AccessTokenResponseJsonSerializerContext.Default.AccessTokenResponse);
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties) => Task.CompletedTask;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        throw new NotSupportedException();
    }
}