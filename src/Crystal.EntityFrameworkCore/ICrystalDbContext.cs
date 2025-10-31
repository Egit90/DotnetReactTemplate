using Crystal.Core;
using Crystal.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crystal.EntityFrameworkCore;

public interface ICrystalDbContext<TUser> where TUser : IdentityUser
{
    DbSet<CrystalRefreshToken> RefreshTokens { get; set; }
}