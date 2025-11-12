using Crystal.Core.Abstractions;
using Crystal.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.EntityFrameworkCore;

public static class CrystalServiceBuilderExtensions
{
    public static CrystalServiceBuilder<TUser, TKey> AddEntityFrameworkStore<TContext, TUser, TKey>(this CrystalServiceBuilder<TUser, TKey> builder)
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>,
        ICrystalUser<TKey>, new()
        where TContext : DbContext, ICrystalDbContext<TUser, TKey>
    {
        builder.IdentityBuilder.AddEntityFrameworkStores<TContext>();
        builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore<TContext, TUser, TKey>>();

        return builder;
    }
}