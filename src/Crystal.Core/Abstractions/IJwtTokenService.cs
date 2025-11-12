using System.Security.Claims;
using Crystal.Core.Models;

namespace Crystal.Core.Abstractions;

/// <summary>
/// Service for creating JWT tokens
/// </summary>
public interface IJwtTokenService<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="user"></param>
    /// <returns>The JWT bearer token and the expiration date</returns>
    (string token, DateTime expiresAt) CreateAccessToken(ClaimsPrincipal user);

    /// <summary>
    /// Creates a bearer refresh token using the given refresh token created by the <see cref="IRefreshTokenManager"/>
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    (string token, DateTime expiresAt) CreateBearerRefreshToken(CrystalRefreshToken<TKey> token);
}
