using WebApi.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebApi;
using WebApi.Endpoints;
using Crystal.Core.Extensions;
using Crystal.Core.Endpoints.SignIn;
using WebApi.Services;
using WebApi.Endpoints.Admin;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMaintenanceService, MaintenanceService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.SetupCrystal(builder.Configuration);
builder.Services.AddScoped<ISignInEndpointEvents<MyUser>, SignInEvents>();

builder.Services.Configure<IdentityOptions>(options => { options.SignIn.RequireConfirmedEmail = true; });

var app = builder.Build();

// Initialize services and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Seed roles
        await RoleSeeder.SeedRolesAsync(services);

        // Initialize maintenance mode from database
        var maintenanceService = services.GetRequiredService<IMaintenanceService>();
        await maintenanceService.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during startup initialization");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseCrystalMiddlewares(); // Add correlation ID and maintenance middleware to all requests
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapCrystalEndpoints();
app.MapAdminEndpoints();

app.MapFallbackToFile("index.html");

try
{
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "An error occurred while starting the application");
}
