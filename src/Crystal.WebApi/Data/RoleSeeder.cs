using Microsoft.AspNetCore.Identity;
using WebApi.Features.UserManagement.Models;

namespace WebApi.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<MyUser>>();

        // Define your roles
        string[] roles = { "Admin", "User", "Moderator" };

        // Create roles if they don't exist
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Optional: Create a default admin user
        await CreateDefaultAdminAsync(userManager);
    }

    private static async Task CreateDefaultAdminAsync(UserManager<MyUser> userManager)
    {
        var adminEmail = "admin@crystal.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new MyUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true // Auto-confirm for admin
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
