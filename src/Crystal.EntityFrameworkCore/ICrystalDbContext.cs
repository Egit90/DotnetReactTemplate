using Crystal.Core;
using Crystal.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crystal.EntityFrameworkCore;

public interface ICrystalDbContext<TUser, TKey>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
{
    DbSet<CrystalRefreshToken> RefreshTokens { get; set; }
}