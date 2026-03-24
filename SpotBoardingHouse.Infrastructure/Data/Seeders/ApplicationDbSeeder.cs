namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public static class ApplicationDbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            ISeeder[] seeders =
            [
                new RoleSeeder()
            ];

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(context, cancellationToken);
            }
        }
    }
}
