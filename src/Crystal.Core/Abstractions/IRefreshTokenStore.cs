using Crystal.Core.Models;

namespace Crystal.Core.Abstractions;

/// <summary>
/// Store for refresh tokens
/// </summary>
public interface IRefreshTokenStore<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<CrystalRefreshToken<TKey>?> FindByUserIdAsync(TKey userId, CancellationToken ct);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task SaveAsync(CrystalRefreshToken<TKey> refreshToken, CancellationToken ct);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task DeleteByUserIdAsync(TKey userId);
}