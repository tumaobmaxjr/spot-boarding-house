using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public class RoomTypeSeeder : ISeeder
    {
        private static readonly (string Name, string? Description, int Min, int Max)[] Defaults =
        [
            ("BedSpace", "Shared dormitory bed, per-person pricing", 1, 8),
            ("Solo", "Private room for single occupant", 1, 1),
            ("Double", "Private room with one large bed for two occupants", 2, 2),
            ("Twin", "Private room with two separate beds", 2, 2),
            ("Family", "Larger room suitable for families", 3, 6),
        ];

        public async Task SeedAsync(
                   ApplicationDbContext context,
                   IConfiguration configuration,
                   CancellationToken cancellationToken = default)
        {
            foreach (var (name, description, min, max) in Defaults)
            {
                var existing = await context.RoomTypes
                    .FirstOrDefaultAsync(rt => rt.Name == name, cancellationToken);
                if (existing == null)
                {
                    context.RoomTypes.Add(new RoomType
                    {
                        Name = name,
                        Description = description,
                        MinOccupants = min,
                        MaxOccupants = max,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    });

                    continue;
                }

                // Strict mode: only normalize occupancy bounds
                existing.MinOccupants = min;
                existing.MaxOccupants = max;
                existing.UpdatedAt = DateTime.UtcNow;

                // Keep existing.Description unchanged
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
