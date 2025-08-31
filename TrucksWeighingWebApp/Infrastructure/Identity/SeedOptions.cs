namespace TrucksWeighingWebApp.Infrastructure.Identity
{
    public class SeedOptions
    {
        public bool MigrateDatabase { get; set; } = true;

        public string[] Roles { get; set; } = Array.Empty<string>();

        public bool EnsureAdmin { get; set; } = true;

        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;

        public string DefaultRoleForNewUsers {  get; set; } = string.Empty;
    }
}
