using Crystal.Core;
using Crystal.Core.Models;
using Crystal.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<MyUser>(options), ICrystalDbContext<MyUser>
{
    public DbSet<CrystalRefreshToken> RefreshTokens { get; set; }
    public DbSet<LogEntry> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyCrystalModel();

        builder.Entity<LogEntry>(entity =>
        {
            entity.ToTable("logs");
            entity.HasNoKey();

            entity.Property(e => e.Timestamp)
                .HasColumnName("timestamp");

            entity.Property(e => e.Level)
                .HasColumnName("level");

            entity.Property(e => e.Message)
                .HasColumnName("message");

            entity.Property(e => e.Exception)
                .HasColumnName("exception");

            entity.Property(e => e.Properties)
                .HasColumnName("log_event");

            entity.Property<string?>("message_template")
                .HasColumnName("message_template");
        });
    }
}

public class LogEntry
{
    public DateTime? Timestamp { get; set; }
    public int? Level { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? Properties { get; set; }
}