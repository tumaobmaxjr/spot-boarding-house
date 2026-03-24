using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public sealed class RoleSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            var hasAdminRole = await context.Roles
                .AnyAsync(r => r.Name == "Admin", cancellationToken);

            if (hasAdminRole)
            {
                return;
            }

            context.Roles.Add(new Role
            {
                Name = "Admin",
                Description = "System administrator"
            });

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
