using Crystal.Core;
using Crystal.Core.Abstractions;
using Crystal.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crystal.EntityFrameworkCore;

public class RefreshTokenStore<TContext, TUser, TKey>(TContext context) : IRefreshTokenStore<TKey>
    where TKey : IEquatable<TKey>
    where TContext : DbContext
    , ICrystalDbContext<TUser, TKey>
    where TUser : IdentityUser<TKey>, ICrystalUser<TKey>
{
    public async Task<CrystalRefreshToken<TKey>?> FindByUserIdAsync(TKey userId, CancellationToken ct)
    {
        return await context.RefreshTokens.FindAsync(userId);
    }

    public async Task SaveAsync(CrystalRefreshToken<TKey> refreshToken, CancellationToken ct)
    {
        // We need to upsert the refresh token
        // Upsert operation is database specific, and it is not supported by EF Core
        // For now there is simple database agnostic solution
        var currentToken = await context.RefreshTokens.FindAsync(refreshToken.UserId, ct);
        if (currentToken is null)
        {
            await context.RefreshTokens.AddAsync(refreshToken, ct);
        }
        else
        {
            currentToken.RefreshToken = refreshToken.RefreshToken;
            currentToken.ExpiresAt = refreshToken.ExpiresAt;
        }

        await context.SaveChangesAsync(ct);
    }

    public Task DeleteByUserIdAsync(TKey userId)
    {
        return context.RefreshTokens.Where(t => t.UserId!.Equals(userId)).ExecuteDeleteAsync();
    }
}