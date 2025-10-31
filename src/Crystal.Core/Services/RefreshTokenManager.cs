using Crystal.Core.Abstractions;
using Crystal.Core.Models;
using Crystal.Core.Options;
﻿using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Crystal.Core.Services;


/// <summary>
/// Default implementation of IRefreshTokenManager
/// </summary>
public class RefreshTokenManager : IRefreshTokenManager
{
    public IOptions<CrystalOptions> Options { get; }
    private readonly IRefreshTokenStore _refreshTokenStore;

    public RefreshTokenManager(IRefreshTokenStore refreshTokenStore, IOptions<CrystalOptions> options)
    {
        Options = options;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<bool> ValidateAsync(string userId, string token, CancellationToken ct = default)
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

    public async Task<CrystalRefreshToken> CreateTokenAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier);
        if (id == null)
            throw new Exception("The user does not have a name identifier claim!");
        
        var refreshToken = new CrystalRefreshToken
        {
            UserId = id.Value,
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(Options.Value.JwtBearer.RefreshTokenExpireInHours)
        };

        await _refreshTokenStore.SaveAsync(refreshToken, ct);

        return refreshToken;
    }

    public Task ClearTokenAsync(string? userId)
    {
        if (userId is null) return Task.CompletedTask;

        return _refreshTokenStore.DeleteByUserIdAsync(userId);
    }
}

