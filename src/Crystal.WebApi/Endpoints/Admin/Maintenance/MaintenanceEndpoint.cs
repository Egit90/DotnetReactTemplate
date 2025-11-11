using WebApi.Services;

namespace WebApi.Endpoints.Admin.Maintenance;

public static class MaintenanceEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/maintenance/enable", EnableMaintenance);
        group.MapPost("/maintenance/disable", DisableMaintenance);
        group.MapGet("/maintenance/status", GetMaintenanceStatus);
    }

    private static async Task<IResult> EnableMaintenance(IMaintenanceService maintenanceService)
    {
        await maintenanceService.EnableAsync();
        return Results.Ok(new { enabled = true, message = "Maintenance mode enabled." });
    }

    private static async Task<IResult> DisableMaintenance(IMaintenanceService maintenanceService)
    {
        await maintenanceService.DisableAsync();
        return Results.Ok(new { enabled = false, message = "Maintenance mode disabled." });
    }

    private static IResult GetMaintenanceStatus(IMaintenanceService maintenanceService)
    {
        return Results.Ok(new { enabled = maintenanceService.IsEnabled });
    }
}
