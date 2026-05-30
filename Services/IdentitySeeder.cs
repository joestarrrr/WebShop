using Microsoft.AspNetCore.Identity;
using WebShop.Data;
using WebShop.Models;

namespace WebShop.Services;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@webshop.local";
        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!dbContext.Products.Any())
        {
            dbContext.Products.AddRange(
                new Product { Name = "Keyboard", Price = 499m, Description = "Simple mechanical keyboard" },
                new Product { Name = "Mouse", Price = 299m, Description = "Wireless mouse" },
                new Product { Name = "Monitor", Price = 1999m, Description = "24-inch monitor" }
            );

            await dbContext.SaveChangesAsync();
        }
    }
}
