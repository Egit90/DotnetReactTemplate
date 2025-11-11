using WebApi.Data;

namespace WebApi.Services;

public interface IMaintenanceService
{
    bool IsEnabled { get; }
    Task EnableAsync(string? enabledBy = null, string? reason = null);
    Task DisableAsync();
    Task InitializeAsync();
}

public class MaintenanceService(IServiceScopeFactory _scopeFactory, ILogger<MaintenanceService> _logger) : IMaintenanceService
{
    private volatile bool _cachedState;

    public bool IsEnabled => _cachedState;

    public async Task InitializeAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var settings = await dbContext.SystemSettings.FindAsync(1);
            if (settings == null)
            {
                // Create default settings
                settings = new SystemSettings
                {
                    Id = 1,
                    Settings = new AppSettings
                    {
                        MaintenanceMode = new MaintenanceModeSettings { Enabled = false }
                    },
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.SystemSettings.Add(settings);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Created default system settings");
            }

            _cachedState = settings.Settings.MaintenanceMode.Enabled;
            _logger.LogInformation("Maintenance mode initialized: {Enabled}", _cachedState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize maintenance mode, defaulting to disabled");
            _cachedState = false;
        }
    }

    public async Task EnableAsync(string? enabledBy = null, string? reason = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var settings = await dbContext.SystemSettings.FindAsync(1);
        if (settings != null)
        {
            settings.Settings.MaintenanceMode.Enabled = true;
            settings.Settings.MaintenanceMode.EnabledAt = DateTime.UtcNow;
            settings.Settings.MaintenanceMode.EnabledBy = enabledBy;
            settings.Settings.MaintenanceMode.Reason = reason;
            settings.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            _cachedState = true;
            _logger.LogWarning("Maintenance mode ENABLED by {User}. Reason: {Reason}", enabledBy ?? "Unknown", reason ?? "None provided");
        }
    }

    public async Task DisableAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var settings = await dbContext.SystemSettings.FindAsync(1);
        if (settings != null)
        {
            settings.Settings.MaintenanceMode.Enabled = false;
            settings.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
            _cachedState = false;
            _logger.LogInformation("Maintenance mode DISABLED");
        }
    }
}
