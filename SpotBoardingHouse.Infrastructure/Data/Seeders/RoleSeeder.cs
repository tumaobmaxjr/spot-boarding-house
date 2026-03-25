using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public sealed class RoleSeeder : ISeeder
    {
        public async Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var hasAdminRole = await context.Roles
                .AnyAsync(r => r.Name == "Admin" && !r.IsDeleted, cancellationToken);

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
