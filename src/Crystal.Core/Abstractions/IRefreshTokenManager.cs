using System.Security.Claims;
using Crystal.Core.Models;

namespace Crystal.Core.Abstractions;

/// <summary>
/// Manager for refresh tokens
/// </summary>
public interface IRefreshTokenManager
{
    /// <summary>
    /// Validates a refresh token associated with a user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <param name="ct"></param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateAsync(string userId, string token, CancellationToken ct = default);

    /// <summary>
    /// Creates a refresh token for a user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<CrystalRefreshToken> CreateTokenAsync(ClaimsPrincipal user, CancellationToken ct = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task ClearTokenAsync(string? userId);
}
