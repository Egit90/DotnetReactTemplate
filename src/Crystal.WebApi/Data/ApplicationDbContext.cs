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
    public DbSet<SystemSettings> SystemSettings { get; set; }

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

            entity.Property(e => e.MessageTemplate)
                .HasColumnName("message_template");

            // jsonb → POCO mapping
            entity.Property(e => e.LogEvent)
                .HasColumnName("log_event")
                .HasColumnType("jsonb");
        });

        builder.Entity<SystemSettings>(entity =>
        {
            entity.OwnsOne(s => s.Settings, settings =>
            {
                settings.ToJson();
                settings.OwnsOne(a => a.MaintenanceMode);
            });
        });
    }
}

public class LogEntry
{
    public DateTime? Timestamp { get; set; }
    public int? Level { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }

    // Map jsonb to a POCO
    public LogEvent? LogEvent { get; set; }
    public string? MessageTemplate { get; set; }
}

public class LogEvent
{
    public string? Level { get; set; }
    public string? Exception { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public LogProperties? Properties { get; set; }
    public string? MessageTemplate { get; set; }
}

public class LogProperties
{
    public EventId? EventId { get; set; }

    // Common properties
    public string? Application { get; set; }
    public string? MachineName { get; set; }
    public string? SourceContext { get; set; }

    // Optional ones (may appear or not)
    public string? Elapsed { get; set; }
    public string? Error { get; set; }
    public string? NewLine { get; set; }
    public string? Newline { get; set; }  // Some logs have lowercase
    public string? RequestId { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }

    // Contextual fields
    public string? ContextType { get; set; }
    public string? CommandText { get; set; }
    public string? CommandType { get; set; }
    public int? CommandTimeout { get; set; }

    public string? ConnectionId { get; set; }
    public string? RemoteIpAddress { get; set; }
}

public class EventId
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class SystemSettings
{
    public int Id { get; set; }
    public AppSettings Settings { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class AppSettings
{
    public MaintenanceModeSettings MaintenanceMode { get; set; } = new();
}

public class MaintenanceModeSettings
{
    public bool Enabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public string? EnabledBy { get; set; }
    public string? Reason { get; set; }
}
