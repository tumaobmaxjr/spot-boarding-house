using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public sealed class RoleSeeder : ISeeder
    {
        public async Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {

            if (!await context.Roles.AnyAsync(r => r.Name == "Admin" && !r.IsDeleted, cancellationToken))
            {
                context.Roles.Add(new Role
                {
                    Name = "Admin",
                    Description = "System administrator"
                });
            }

            if (!await context.Roles.AnyAsync(r => r.Name == "Owner" && !r.IsDeleted, cancellationToken))
            {
                context.Roles.Add(new Role
                {
                    Name = "Owner",
                    Description = "Boarding house owner"
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
