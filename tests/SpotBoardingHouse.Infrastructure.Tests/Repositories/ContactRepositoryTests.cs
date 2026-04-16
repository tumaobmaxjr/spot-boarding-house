using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;
using SpotBoardingHouse.Infrastructure.Data;
using SpotBoardingHouse.Infrastructure.Repositories;

namespace SpotBoardingHouse.Infrastructure.Tests.Repositories
{
    public class ContactRepositoryTests
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
        public async Task GetAllByBoardingHouseIdAsync_ShouldOrderPrimaryThenPhone_AndExcludeDeleted()
        {
            using var context = CreateContext();
            var repo = new ContactRepository(context);
            var boardingHouseId = Guid.NewGuid();

            await context.Contacts.AddRangeAsync(
            [
                new Contact { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, PhoneNumber = "09170000002", Network = Network.Globe, IsPrimary = false, IsDeleted = false },
                new Contact { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, PhoneNumber = "09170000001", Network = Network.Smart, IsPrimary = true, IsDeleted = false },
                new Contact { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, PhoneNumber = "09170000000", Network = Network.TNT, IsPrimary = false, IsDeleted = true },
                new Contact { Id = Guid.NewGuid(), BoardingHouseId = Guid.NewGuid(), PhoneNumber = "09179999999", Network = Network.DITO, IsPrimary = true, IsDeleted = false }
            ],
            TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = (await repo.GetAllByBoardingHouseIdAsync(boardingHouseId)).ToList();

            result.Should().HaveCount(2);
            result[0].IsPrimary.Should().BeTrue();
            result[0].PhoneNumber.Should().Be("09170000001");
            result[1].PhoneNumber.Should().Be("09170000002");
        }

        [Fact]
        public async Task PhoneExistsAsync_ShouldRespectExcludedId_AndDeletedFilter()
        {
            using var context = CreateContext();
            var repo = new ContactRepository(context);
            var boardingHouseId = Guid.NewGuid();
            var existingId = Guid.NewGuid();

            await context.Contacts.AddRangeAsync(
            [
                new Contact { Id = existingId, BoardingHouseId = boardingHouseId, PhoneNumber = "09171234567", Network = Network.Globe, IsDeleted = false },
                new Contact { Id = Guid.NewGuid(), BoardingHouseId = boardingHouseId, PhoneNumber = "09179999999", Network = Network.Smart, IsDeleted = true }
            ],
            TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var existsWithoutExclude = await repo.PhoneExistsAsync(boardingHouseId, "09171234567");
            var existsWithExclude = await repo.PhoneExistsAsync(boardingHouseId, "09171234567", existingId);
            var deletedExists = await repo.PhoneExistsAsync(boardingHouseId, "09179999999");

            existsWithoutExclude.Should().BeTrue();
            existsWithExclude.Should().BeFalse();
            deletedExists.Should().BeFalse();
        }

        [Fact]
        public async Task GetPrimaryByBoardingHouseId_ShouldRespectExcludedId()
        {
            using var context = CreateContext();
            var repo = new ContactRepository(context);
            var boardingHouseId = Guid.NewGuid();
            var primaryA = new Contact
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09170000001",
                Network = Network.Globe,
                IsPrimary = true,
                IsDeleted = false
            };
            var primaryB = new Contact
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09170000002",
                Network = Network.Smart,
                IsPrimary = true,
                IsDeleted = false
            };

            await context.Contacts.AddRangeAsync([primaryA, primaryB], TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await repo.GetPrimaryByBoardingHouseId(boardingHouseId, primaryA.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(primaryB.Id);
        }
    }
}
