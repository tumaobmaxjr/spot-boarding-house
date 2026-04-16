using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;
using SpotBoardingHouse.Infrastructure.Repositories;

namespace SpotBoardingHouse.Infrastructure.Tests.Repositories
{
    public class MediaRepositoryTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAllByBoardingHouseIdAsync_ShouldReturnOnlyActiveMedia_OrderedByPrimary()
        {
            using var context = CreateContext();
            var repo = new MediaRepository(context);
            var boardingHouseId = Guid.NewGuid();

            await context.Media.AddRangeAsync(
            [
                new Media { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, Url = "https://example.com/2.jpg", IsPrimary = false, DisplayOrder = 2, IsDeleted = false },
                new Media { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, Url = "https://example.com/1.jpg", IsPrimary = true, DisplayOrder = 1, IsDeleted = false },
                new Media { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, Url = "https://example.com/deleted.jpg", IsPrimary = true, DisplayOrder = 0, IsDeleted = true },
                new Media { Id = Guid.NewGuid(), BoardingHouseId = Guid.NewGuid(), Url = "https://example.com/other.jpg", IsPrimary = true, DisplayOrder = 1, IsDeleted = false }
            ],
            TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = (await repo.GetAllByBoardingHouseIdAsync(boardingHouseId)).ToList();

            result.Should().HaveCount(2);
            result[0].IsPrimary.Should().BeTrue();
            result[0].Url.Should().Be("https://example.com/1.jpg");
        }

        [Fact]
        public async Task GetAllByRoomIdAsync_ShouldReturnOnlyActiveMedia_OrderedByPrimary()
        {
            using var context = CreateContext();
            var repo = new MediaRepository(context);
            var roomId = Guid.NewGuid();

            await context.Media.AddRangeAsync(
            [
                new Media { Id = Guid.NewGuid(), RoomId = roomId, Url = "https://example.com/r2.jpg", IsPrimary = false, DisplayOrder = 2, IsDeleted = false },
                new Media { Id = Guid.NewGuid(), RoomId = roomId, Url = "https://example.com/r1.jpg", IsPrimary = true, DisplayOrder = 1, IsDeleted = false },
                new Media { Id = Guid.NewGuid(), RoomId = roomId, Url = "https://example.com/r-deleted.jpg", IsPrimary = true, DisplayOrder = 0, IsDeleted = true }
            ],
            TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = (await repo.GetAllByRoomIdAsync(roomId)).ToList();

            result.Should().HaveCount(2);
            result[0].IsPrimary.Should().BeTrue();
            result[0].Url.Should().Be("https://example.com/r1.jpg");
        }

        [Fact]
        public async Task GetPrimaryByBoardingHouseIdAsync_ShouldRespectExcludedId()
        {
            using var context = CreateContext();
            var repo = new MediaRepository(context);
            var boardingHouseId = Guid.NewGuid();
            var primaryA = new Media
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/a.jpg",
                IsPrimary = true,
                DisplayOrder = 1,
                IsDeleted = false
            };
            var primaryB = new Media
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/b.jpg",
                IsPrimary = true,
                DisplayOrder = 2,
                IsDeleted = false
            };

            await context.Media.AddRangeAsync([primaryA, primaryB], TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await repo.GetPrimaryByBoardingHouseIdAsync(boardingHouseId, primaryA.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(primaryB.Id);
        }

        [Fact]
        public async Task GetPrimaryByRoomIdAsync_ShouldRespectDeletedFilter()
        {
            using var context = CreateContext();
            var repo = new MediaRepository(context);
            var roomId = Guid.NewGuid();

            await context.Media.AddRangeAsync(
            [
                new Media { Id = Guid.NewGuid(), RoomId = roomId, Url = "https://example.com/deleted-primary.jpg", IsPrimary = true, DisplayOrder = 1, IsDeleted = true },
                new Media { Id = Guid.NewGuid(), RoomId = roomId, Url = "https://example.com/active-primary.jpg", IsPrimary = true, DisplayOrder = 2, IsDeleted = false }
            ],
            TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await repo.GetPrimaryByRoomIdAsync(roomId);

            result.Should().NotBeNull();
            result!.Url.Should().Be("https://example.com/active-primary.jpg");
        }
    }
}
