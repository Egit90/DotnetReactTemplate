using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Crystal.Core.Services;


/// <summary>
/// Default implementation of IRefreshTokenManager
/// </summary>
public class RefreshTokenManager<TKey>(IRefreshTokenStore<TKey> _refreshTokenStore, IOptions<CrystalOptions> Options) : IRefreshTokenManager<TKey> where TKey : IEquatable<TKey>
{
    public async Task<bool> ValidateAsync(TKey userId, string token, CancellationToken ct = default)
    {
        var refreshToken = await _refreshTokenStore.FindByUserIdAsync(userId, ct);

        //Validate the refresh token
        if (refreshToken is null
            || refreshToken.RefreshToken != token
            || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    public async Task<CrystalRefreshToken<TKey>> CreateTokenAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier);
        if (id == null)
            throw new Exception("The user does not have a name identifier claim!");

        // Convert string claim value to TKey
        TKey userId = typeof(TKey) == typeof(Guid)
                      ? (TKey)(object)Guid.Parse(id.Value)
                      : (TKey)Convert.ChangeType(id.Value, typeof(TKey));

        var refreshToken = new CrystalRefreshToken<TKey>
        {
            UserId = userId,
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(Options.Value.JwtBearer.RefreshTokenExpireInHours)
        };

        await _refreshTokenStore.SaveAsync(refreshToken, ct);

        return refreshToken;
    }

    public Task ClearTokenAsync(TKey? userId)
    {
        if (userId is null) return Task.CompletedTask;

        return _refreshTokenStore.DeleteByUserIdAsync(userId);
    }
}

