using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;
using SpotBoardingHouse.Infrastructure.Data;
using SpotBoardingHouse.Infrastructure.Repositories;

namespace SpotBoardingHouse.Infrastructure.Tests.Repositories
{
    public class BoardingHouseRepositoryTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private static BoardingHouseRepository CreateRepository(ApplicationDbContext context)
        {
            return new BoardingHouseRepository(context);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistBoardingHouse()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Test BH",
                AllowedGender = Gender.Mixed,
                Address = "123 Test St",
                Latitude = 14.5,
                Longitude = 121.0
            };

            var result = await repo.AddAsync(boardingHouse);

            result.Id.Should().Be(boardingHouse.Id);
            result.Name.Should().Be("Test BH");

            var saved = await context.BoardingHouses.FindAsync([boardingHouse.Id], TestContext.Current.CancellationToken);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Test BH");
        }

        [Fact]
        public async Task GetAllByUserIdAsync_ShouldReturnOnlyUserBoardingHouses()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            await repo.AddAsync(new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userA,
                Name = "BH A1",
                AllowedGender = Gender.Male,
                Address = "Addr A1"
            });

            await repo.AddAsync(new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userA,
                Name = "BH A2",
                AllowedGender = Gender.Female,
                Address = "Addr A2"
            });

            await repo.AddAsync(new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userB,
                Name = "BH B1",
                AllowedGender = Gender.Mixed,
                Address = "Addr B1"
            });

            var result = (await repo.GetAllByUserIdAsync(userA)).ToList();

            result.Should().HaveCount(2);
            result.All(bh => bh.UserId == userA).Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldSetIsDeletedAndDeletedAt()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "To Delete",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            };

            await repo.AddAsync(boardingHouse);
            await repo.DeleteAsync(boardingHouse.Id);

            var deleted = await context.BoardingHouses.FindAsync([boardingHouse.Id], TestContext.Current.CancellationToken);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().BeTrue();
            deleted.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task GetAllByUserIdAsync_ShouldExcludeDeleted()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var userId = Guid.NewGuid();

            var active = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Active BH",
                AllowedGender = Gender.Mixed,
                Address = "Addr 1"
            };

            var deleted = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Deleted BH",
                AllowedGender = Gender.Male,
                Address = "Addr 2",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };

            await context.BoardingHouses.AddRangeAsync([active, deleted], TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = (await repo.GetAllByUserIdAsync(userId)).ToList();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Active BH");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var deleted = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Deleted BH",
                AllowedGender = Gender.Mixed,
                Address = "Addr",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };

            await context.BoardingHouses.AddAsync(deleted, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await repo.GetByIdAsync(deleted.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldIncludeUser_WhenFound()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Owner"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                RoleId = role.Id,
                Role = role,
                FirstName = "Max",
                LastName = "Owner",
                Email = "owner@test.com",
                PasswordHash = "hash"
            };

            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                Name = "BH with owner",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            };

            await context.Roles.AddAsync(role, TestContext.Current.CancellationToken);
            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.BoardingHouses.AddAsync(boardingHouse, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await repo.GetByIdAsync(boardingHouse.Id);

            result.Should().NotBeNull();
            result!.User.Should().NotBeNull();
            result.User.FirstName.Should().Be("Max");
            result.User.LastName.Should().Be("Owner");
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenActive()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var active = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Active BH",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            };

            await context.BoardingHouses.AddAsync(active, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var exists = await repo.ExistsAsync(active.Id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenDeleted()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var deleted = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Deleted BH",
                AllowedGender = Gender.Mixed,
                Address = "Addr",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };

            await context.BoardingHouses.AddAsync(deleted, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var exists = await repo.ExistsAsync(deleted.Id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NameExistsForUserAsync_ShouldReturnTrue_WhenNameExists()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var userId = Guid.NewGuid();
            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Duplicate Name",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            };

            await context.BoardingHouses.AddAsync(boardingHouse, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var exists = await repo.NameExistsForUserAsync("Duplicate Name", userId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task NameExistsForUserAsync_ShouldReturnFalse_WhenExcludedIdMatches()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var userId = Guid.NewGuid();
            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Same Name",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            };

            await context.BoardingHouses.AddAsync(boardingHouse, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var exists = await repo.NameExistsForUserAsync("Same Name", userId, boardingHouse.Id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldPersistUpdatedFields()
        {
            using var context = CreateContext();
            var repo = CreateRepository(context);

            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Name = "Before",
                AllowedGender = Gender.Mixed,
                Address = "Addr",
                FacebookLink = "https://old.com"
            };

            await context.BoardingHouses.AddAsync(boardingHouse, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            boardingHouse.Name = "After";
            boardingHouse.FacebookLink = "https://new.com";

            await repo.UpdateAsync(boardingHouse);

            var updated = await context.BoardingHouses.FindAsync([boardingHouse.Id], TestContext.Current.CancellationToken);
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("After");
            updated.FacebookLink.Should().Be("https://new.com");
        }
    }
}
