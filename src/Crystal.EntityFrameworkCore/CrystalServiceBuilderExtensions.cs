using Crystal.Core.Abstractions;
using Crystal.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.EntityFrameworkCore;

public static class CrystalServiceBuilderExtensions
{
    public static CrystalServiceBuilder<TUser> AddEntityFrameworkStore<TContext, TUser>(this CrystalServiceBuilder<TUser> builder)
        where TUser : IdentityUser<Guid>, ICrystalUser, new()
        where TContext : DbContext, ICrystalDbContext<TUser>
    {
        builder.IdentityBuilder.AddEntityFrameworkStores<TContext>();
        builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore<TContext, TUser>>();

        return builder;
    }
}