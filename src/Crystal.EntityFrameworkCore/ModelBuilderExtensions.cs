using Crystal.Core;
using Crystal.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Crystal.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    public static void ApplyCrystalModel(this ModelBuilder builder)
    {
        builder.Entity<CrystalRefreshToken>(b =>
        {
            b.ToTable("CrystalRefreshTokens");
            b.HasKey(x => x.UserId);
            b.Property(x => x.RefreshToken).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
        });
    }
}