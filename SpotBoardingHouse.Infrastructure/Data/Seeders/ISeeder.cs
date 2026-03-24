namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public interface ISeeder
    {
        Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken);
    }
}
