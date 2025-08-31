using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Infrastructure.Identity
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var options = sp.GetRequiredService<IOptions<SeedOptions>>().Value;

            // DB migrations
            if (options.MigrateDatabase)
            {
                await db.Database.MigrateAsync();
            }

            // Roles
            foreach (var role in options.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Admin
            if (options.EnsureAdmin)
            {
                if (string.IsNullOrWhiteSpace(options.AdminEmail) || string.IsNullOrWhiteSpace(options.AdminPassword))
                {
                    throw new InvalidOperationException("Admin email/password not set in configuration.");
                }

                var admin = await userManager.FindByEmailAsync(options.AdminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = options.AdminEmail,
                        Email = options.AdminEmail,
                        EmailConfirmed = true,
                        FullName = "System Admin",
                        AdminNote = "Seeded admin user"
                    };

                    var result = await userManager.CreateAsync(admin, options.AdminPassword);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Can't create admin");
                    }
                }

                if (!await userManager.IsInRoleAsync(admin, RoleNames.Admin))
                {
                    await userManager.AddToRoleAsync(admin, RoleNames.Admin);
                }
            }

            // Other Users default role
            if (!string.IsNullOrWhiteSpace(options.DefaultRoleForNewUsers))
            {
                var defaultRole = options.DefaultRoleForNewUsers;

                if (!await roleManager.RoleExistsAsync(defaultRole))
                {
                    throw new InvalidOperationException($"Default role '{defaultRole}' does not exist.");
                }

                var userIds = await userManager.Users
                    .AsNoTracking()
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var userId in userIds)
                {
                    var user = await userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        continue;
                    }

                    // Admins not considered
                    if (await userManager.IsInRoleAsync(user, RoleNames.Admin))
                    {
                        continue;
                    }

                    var userRoles = await userManager.GetRolesAsync(user);

                    if (userRoles.Count == 0)
                    {
                        await userManager.AddToRoleAsync(user, defaultRole);                        
                    }
                }
            }            

        }        
    }
}
