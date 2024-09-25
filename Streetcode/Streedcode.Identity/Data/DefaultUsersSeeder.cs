using Microsoft.AspNetCore.Identity;
using Streedcode.Identity.Models;

namespace Streedcode.Identity.Data
{
    public class DefaultUsersSeeder
    {
        public static async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Створення ролей
            var defaultRoles = new[] { "Admin", "User" };
            foreach (var role in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Створення дефолтного адміністратора
            var defaultAdminEmail = "admin@example.com";
            var defaultAdmin = new ApplicationUser
            {
                UserName = defaultAdminEmail,
                Email = defaultAdminEmail,
                Name = "Default Admin",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(defaultAdminEmail) == null)
            {
                var result = await userManager.CreateAsync(defaultAdmin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultAdmin, "Admin");
                }
            }

            // Створення дефолтного користувача
            var defaultUserEmail = "user@example.com";
            var defaultUser = new ApplicationUser
            {
                UserName = defaultUserEmail,
                Email = defaultUserEmail,
                Name = "Default User",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(defaultUserEmail) == null)
            {
                var result = await userManager.CreateAsync(defaultUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(defaultUser, "User");
                }
            }
        }
    }
}
