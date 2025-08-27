using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Services
{
    public class IdentitySeed
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Inspector"};

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "albeaz.abbas@gmail.com";
            var admin = await userManager.FindByNameAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true,
                    AdminNote = "Seeded admin user"
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

        }        
    }
}
