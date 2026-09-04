using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureJwtApi.Models;

namespace SecureJwtApi.Data;

/// <summary>
/// Static class for seeding initial roles and users.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Ensures the Admin role exists and creates a default admin user.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // 1. Ensure the "Admin" role exists
        var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
        if (!adminRoleExists)
        {
            // Create the role
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to create Admin role: {string.Join(", ", roleResult.Errors)}");
            }
        }

        // 2. Ensure a default admin user exists
        const string adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            // Create the user
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail
            };
            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors)}");
            }

            // Assign the Admin role
            var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addRoleResult.Succeeded)
            {
                throw new Exception($"Failed to assign Admin role to admin user: {string.Join(", ", addRoleResult.Errors)}");
            }
        }
        else
        {
            // If the user already exists but might not have the Admin role, ensure it's assigned.
            var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
            if (!isInRole)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // (Optional) You could also create a default "User" role and assign it to every new user,
        // but that's not required for this demo.
    }
}