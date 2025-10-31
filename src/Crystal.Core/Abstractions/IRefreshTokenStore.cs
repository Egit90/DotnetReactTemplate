using Crystal.Core.Models;

namespace Crystal.Core.Abstractions;

/// <summary>
/// Store for refresh tokens
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<CrystalRefreshToken?> FindByUserIdAsync(string userId, CancellationToken ct);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task SaveAsync(CrystalRefreshToken refreshToken, CancellationToken ct);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task DeleteByUserIdAsync(string userId);
}