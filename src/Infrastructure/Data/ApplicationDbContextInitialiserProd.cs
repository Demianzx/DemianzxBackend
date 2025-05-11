using DemianzxBackend.Domain.Constants;
using DemianzxBackend.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DemianzxBackend.Infrastructure.Data;

public static class ProductionInitializer
{
    public static async Task InitializeProductionDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            // Seed roles
            var administratorRole = new IdentityRole(Roles.Administrator);
            if (!await roleManager.RoleExistsAsync(Roles.Administrator))
            {
                await roleManager.CreateAsync(administratorRole);
            }

            // Seed admin user
            var adminUsername = "Demianzx";
            var adminUser = await userManager.FindByNameAsync(adminUsername);

            if (adminUser == null)
            {
                var administrator = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = "demianzito.gaming@gmail.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(administrator, "Administrator1!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(administrator, Roles.Administrator);
                    logger.LogInformation("Admin user created successfully in production environment");
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the production database");
            throw;
        }
    }
}
