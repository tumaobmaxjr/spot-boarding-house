using Microsoft.Extensions.Configuration;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public interface ISeeder
    {
        Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration,
            CancellationToken cancellationToken);
    }
}
