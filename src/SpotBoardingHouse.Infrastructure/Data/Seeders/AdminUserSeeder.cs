using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Infrastructure.Data.Seeders
{
    public sealed class AdminUserSeeder : ISeeder
    {
        public async Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var emailFromConfig = configuration["SeedAdmin:Email"];
            var adminEmail = (string.IsNullOrWhiteSpace(emailFromConfig)
                    ? "admin@spotboardinghouse.com"
                    : emailFromConfig)
                .Trim().ToLowerInvariant();

            // env takes priority over appsettings
            var passwordFromEnv = Environment.GetEnvironmentVariable("SBH_ADMIN_PASSWORD");
            var passwordFromConfig = configuration["SeedAdmin:Password"];
            var adminPassword = !string.IsNullOrWhiteSpace(passwordFromEnv)
                ? passwordFromEnv
                : passwordFromConfig;

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                // fail-safe: skip seeding if password isn't provided
                return;
            }

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin" && !r.IsDeleted, cancellationToken);

            if (adminRole is null)
            {
                return;
            }

            var hasAdminUser = await context.Users
                .AnyAsync(u => !u.IsDeleted && u.Email.ToLower() == adminEmail, cancellationToken);

            if (hasAdminUser)
            {
                return;
            }

            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                RoleId = adminRole.Id,
                FirstName = "System",
                LastName = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
