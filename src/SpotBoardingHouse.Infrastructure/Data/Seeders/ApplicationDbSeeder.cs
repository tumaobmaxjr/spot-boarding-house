using Microsoft.Extensions.Configuration;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public static class ApplicationDbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            ISeeder[] seeders =
            [
                new RoleSeeder(),
                new AdminUserSeeder()
            ];

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(context, configuration, cancellationToken);
            }
        }
    }
}
