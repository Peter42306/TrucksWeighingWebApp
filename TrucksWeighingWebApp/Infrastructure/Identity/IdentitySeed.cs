using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Infrastructure.Identity
{
    public static class IdentitySeed
    {
        private static string GetSeedMarkerPath(IServiceProvider sp)
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            return env.IsDevelopment() 
                ? Path.Combine(env.ContentRootPath, ".identity-seeded") 
                : "/var/lib/trucks/.identity-seeded";
        }

        public static async Task SeedAsync(IServiceProvider sp)
        {
            var marker = GetSeedMarkerPath(sp);
            if (File.Exists(marker))
            {
                return;
            }

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

            // One time creation of Admin
            if (options.EnsureAdmin)
            {
                var adminEmail = options.AdminEmail; //Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? options.AdminEmail;
                var adminPassword = options.AdminPassword; //Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? options.AdminPassword;

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException("Seed: AdminEmail/AdminPassword are not set.");
                }

                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FullName = "System Admin",
                        AdminNote = "Seeded admin user"
                    };

                    var result = await userManager.CreateAsync(admin, adminPassword);
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

            // Creation marker that directory exists
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(marker)!);
                await File.WriteAllTextAsync(marker, $"Seeded at {DateTime.UtcNow:O}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: cannot write seed marker at {marker}: {ex.Message}");
            }
                        


        }        
    }
}
