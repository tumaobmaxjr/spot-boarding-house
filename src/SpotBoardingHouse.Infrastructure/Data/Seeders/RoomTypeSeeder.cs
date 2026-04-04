using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public class RoomTypeSeeder : ISeeder
    {
        private static readonly (string Name, string? Description)[] Defaults = new[]
        {
            ("BedSpace", "Shared dormitory bed, per-person pricing"),
            ("Solo", "Private room for single occupant"),
            ("Double", "Private room with one large bed for two occupants"),
            ("Twin", "Private room with two separate beds"),
            ("Family", "Larger room suitable for families"),
        };

        public async Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            foreach (var (name, desc) in Defaults)
            {
                if (!await context.RoomTypes.AnyAsync(rt => rt.Name == name && !rt.IsDeleted, cancellationToken))
                {
                    context.RoomTypes.Add(new RoomType
                    {
                        Name = name,
                        Description = desc
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
