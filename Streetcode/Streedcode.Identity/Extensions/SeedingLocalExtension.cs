using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Streedcode.Identity.Data;
using Streedcode.Identity.Models;
using Streetcode.Identity.Models.Additional;

namespace Streedcode.Identity.Extensions;

public static class SeedingLocalExtension
{
    public static async Task SeedDataAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var adminConfig = app.Configuration.GetSection(nameof(AdminConfiguration)).Get<AdminConfiguration>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
            }

            //Seed User role
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = "User" });
            }

            if (!dbContext.Users.Any())
            {
                var adminUser = new ApplicationUser
                {
                    Name = adminConfig.Name,
                    Surname = adminConfig.Surname,
                    Email = adminConfig.Email,
                    UserName = adminConfig.Login,
                    Role = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, adminConfig.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                // Create users
                var users = new List<ApplicationUser>
                    {
                        new ApplicationUser { Name = "John", Surname = "Doe", Email = "john@example.com", UserName = "john1", Role = "User" },
                        new ApplicationUser { Name = "Jane", Surname = "Smith", Email = "jane@example.com", UserName = "jane1", Role = "User" }
                    };

                foreach (var user in users)
                {
                    var res = await userManager.CreateAsync(user, "User@123");

                    if (result.Succeeded)
                    {
                        // Role added
                        await userManager.AddToRoleAsync(user, user.Role.ToString());
                    }
                    else
                    {
                        // Exception
                        foreach (var error in res.Errors)
                        {
                            Console.WriteLine($"Error creating user {user.UserName}: {error.Description}");
                        }
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
