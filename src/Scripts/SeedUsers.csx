#!/usr/bin/env dotnet-script
/*
 * Seed Fake Users Script
 *
 * This script creates 10 fake users in the database for testing purposes.
 *
 * Prerequisites:
 * 1. Install dotnet-script tool globally:
 *    dotnet tool install -g dotnet-script
 *
 * How to run:
 * 1. Navigate to the Scripts directory:
 *    cd src/Scripts
 *
 * 2. Run the script:
 *    dotnet script SeedUsers.csx
 *
 * Note: All fake users will have the password: Password123!
 * The script will skip users that already exist in the database.
 */

#r "nuget: Microsoft.AspNetCore.Identity.EntityFrameworkCore, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 8.0.0"
#r "nuget: Microsoft.EntityFrameworkCore, 8.0.0"
#r "nuget: SQLitePCLRaw.bundle_e_sqlite3, 2.1.6"

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

SQLitePCL.Batteries_V2.Init();

// MyUser class
public class MyUser : IdentityUser
{
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
}

// DbContext
public class AppDbContext : IdentityDbContext<MyUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}

// Main execution
await SeedUsersAsync();

async Task SeedUsersAsync()
{
    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseSqlite("Data Source=../Crystal.WebApi/app.db");

    using var context = new AppDbContext(optionsBuilder.Options);
    var userStore = new UserStore<MyUser>(context);
    var userManager = new UserManager<MyUser>(
        userStore,
        null,
        new PasswordHasher<MyUser>(),
        null,
        null,
        null,
        null,
        null,
        null
    );

    var fakeUsers = new (string UserName, string Email, bool EmailConfirmed, DateTime? LastLogin)[]
    {
        ("john.doe", "john.doe@example.com", true, DateTime.UtcNow.AddDays(-2)),
        ("jane.smith", "jane.smith@example.com", true, DateTime.UtcNow.AddDays(-5)),
        ("bob.wilson", "bob.wilson@example.com", false, null),
        ("alice.johnson", "alice.johnson@example.com", true, DateTime.UtcNow.AddHours(-3)),
        ("charlie.brown", "charlie.brown@example.com", true, DateTime.UtcNow.AddDays(-1)),
        ("diana.prince", "diana.prince@example.com", false, null),
        ("frank.castle", "frank.castle@example.com", true, DateTime.UtcNow.AddDays(-10)),
        ("grace.hopper", "grace.hopper@example.com", true, DateTime.UtcNow.AddDays(-7)),
        ("henry.ford", "henry.ford@example.com", false, null),
        ("iris.west", "iris.west@example.com", true, DateTime.UtcNow.AddHours(-12))
    };

    var password = "Password123!";
    var createdCount = 0;
    var skippedCount = 0;

    foreach (var fakeUser in fakeUsers)
    {
        var existingUser = await userManager.FindByEmailAsync(fakeUser.Email);
        if (existingUser != null)
        {
            Console.WriteLine($"User {fakeUser.Email} already exists. Skipping.");
            skippedCount++;
            continue;
        }

        var user = new MyUser
        {
            UserName = fakeUser.UserName,
            Email = fakeUser.Email,
            EmailConfirmed = fakeUser.EmailConfirmed,
            CreatedOn = DateTime.UtcNow.AddDays(-30 + createdCount * 2),
            LastLoginDate = fakeUser.LastLogin
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            Console.WriteLine($"✓ Created user: {user.Email}");
            createdCount++;
        }
        else
        {
            Console.WriteLine($"✗ Failed to create user {fakeUser.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    Console.WriteLine($"\n=== Summary ===");
    Console.WriteLine($"Created: {createdCount} users");
    Console.WriteLine($"Skipped: {skippedCount} users");
    Console.WriteLine($"Total: {fakeUsers.Length} users");
}