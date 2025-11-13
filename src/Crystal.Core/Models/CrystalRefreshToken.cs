namespace Crystal.Core.Models;

/// <summary>
/// Represents a refresh token
/// </summary>
public class CrystalRefreshToken<TKey>
{
    public required TKey UserId { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}