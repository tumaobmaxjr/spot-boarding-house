using FluentAssertions;
using Moq;
using SpotBoardingHouse.Application.DTOs.BoardingHouses;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Services;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.Tests.Services
{
    public class BoardingHouseServiceTests
    {
        private readonly Mock<IBoardingHouseRepository> _repo = new();
        private readonly BoardingHouseService _service;

        public BoardingHouseServiceTests()
        {
            _service = new BoardingHouseService(_repo.Object);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_ShouldReturnOnlyUserBoardingHouses()
        {
            var userId = Guid.NewGuid();
            var boardingHouses = new List<BoardingHouse>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, Name = "BH 1", AllowedGender = Gender.Mixed, Address = "Addr 1" },
                new() { Id = Guid.NewGuid(), UserId = userId, Name = "BH 2", AllowedGender = Gender.Male, Address = "Addr 2" }
            };

            _repo.Setup(r => r.GetAllByUserIdAsync(userId)).ReturnsAsync(boardingHouses);

            var result = (await _service.GetAllByUserIdAsync(userId)).ToList();

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("BH 1");
            result[1].Name.Should().Be("BH 2");
            result.All(bh => bh.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBoardingHouse_WhenFound()
        {
            var id = Guid.NewGuid();
            var boardingHouse = new BoardingHouse
            {
                Id = id,
                UserId = Guid.NewGuid(),
                Name = "My BH",
                AllowedGender = Gender.Female,
                Address = "123 Main St",
                Latitude = 14.5,
                Longitude = 121.0
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(boardingHouse);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("My BH");
            result.AllowedGender.Should().Be(Gender.Female.ToString());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BoardingHouse?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ShouldCreate_AndMapDto()
        {
            var userId = Guid.NewGuid();
            var dto = new CreateBoardingHouseDto
            {
                Name = "  My BH  ",
                AllowedGender = "mixed",
                Address = "  123 Main St  ",
                Latitude = 14.5995,
                Longitude = 120.9842,
                FacebookLink = "  https://fb.com/test  "
            };

            _repo.Setup(r => r.NameExistsForUserAsync(dto.Name, userId, null)).ReturnsAsync(false);
            _repo.Setup(r => r.AddAsync(It.IsAny<BoardingHouse>()))
                .ReturnsAsync((BoardingHouse bh) => bh);

            var result = await _service.CreateAsync(userId, dto);

            result.UserId.Should().Be(userId);
            result.Name.Should().Be("My BH");
            result.Address.Should().Be("123 Main St");
            result.AllowedGender.Should().Be(Gender.Mixed.ToString());
            result.Latitude.Should().Be(14.5995);
            result.Longitude.Should().Be(120.9842);
            result.FacebookLink.Should().Be("https://fb.com/test");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenNameAlreadyExists()
        {
            var userId = Guid.NewGuid();
            var dto = new CreateBoardingHouseDto
            {
                Name = "My BH",
                AllowedGender = "Mixed",
                Address = "123 Main St",
                Latitude = 14.5995,
                Longitude = 120.9842
            };

            _repo.Setup(r => r.NameExistsForUserAsync("My BH", userId, null)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.CreateAsync(userId, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenGenderInvalid()
        {
            var userId = Guid.NewGuid();
            var dto = new CreateBoardingHouseDto
            {
                Name = "My BH",
                AllowedGender = "Unknown",
                Address = "123 Main St",
                Latitude = 14.5995,
                Longitude = 120.9842
            };

            _repo.Setup(r => r.NameExistsForUserAsync(dto.Name, userId, null)).ReturnsAsync(false);

            Func<Task> act = async () => await _service.CreateAsync(userId, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid gender value*");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdate_AllFields()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existing = new BoardingHouse
            {
                Id = id,
                UserId = userId,
                Name = "Old Name",
                Description = "Old Desc",
                AllowedGender = Gender.Male,
                Address = "Old Address",
                Latitude = 14.0,
                Longitude = 120.0,
                FacebookLink = "https://old.com"
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _repo.Setup(r => r.NameExistsForUserAsync("New Name", userId, id)).ReturnsAsync(false);
            _repo.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(id, userId, new UpdateBoardingHouseDto
            {
                Name = "New Name",
                Description = "New Desc",
                AllowedGender = "Female",
                Address = "New Address",
                Latitude = 14.5,
                Longitude = 121.0,
                FacebookLink = "https://new.com"
            });

            result.Should().NotBeNull();
            result!.Name.Should().Be("New Name");
            result.Description.Should().Be("New Desc");
            result.AllowedGender.Should().Be(Gender.Female.ToString());
            result.Address.Should().Be("New Address");
            result.Latitude.Should().Be(14.5);
            result.Longitude.Should().Be(121.0);
            result.FacebookLink.Should().Be("https://new.com");
        }

        [Fact]
        public async Task UpdateAsync_ShouldOnlyUpdateProvidedFields()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existing = new BoardingHouse
            {
                Id = id,
                UserId = userId,
                Name = "Original Name",
                Description = "Original Desc",
                AllowedGender = Gender.Mixed,
                Address = "Original Address",
                Latitude = 14.0,
                Longitude = 120.0,
                FacebookLink = "https://original.com"
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _repo.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(id, userId, new UpdateBoardingHouseDto
            {
                Name = "Updated Name"
            });

            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Name");
            result.Description.Should().Be("Original Desc");
            result.AllowedGender.Should().Be(Gender.Mixed.ToString());
            result.Address.Should().Be("Original Address");
            result.Latitude.Should().Be(14.0);
            result.Longitude.Should().Be(120.0);
            result.FacebookLink.Should().Be("https://original.com");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenNotFound()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BoardingHouse?)null);

            var result = await _service.UpdateAsync(id, userId, new UpdateBoardingHouseDto());

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDuplicateName()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existing = new BoardingHouse
            {
                Id = id,
                UserId = userId,
                Name = "Existing BH",
                AllowedGender = Gender.Mixed,
                Address = "123 Main St"
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _repo.Setup(r => r.NameExistsForUserAsync("New Name", userId, id)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.UpdateAsync(id, userId, new UpdateBoardingHouseDto
            {
                Name = "New Name"
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenGenderInvalid()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existing = new BoardingHouse
            {
                Id = id,
                UserId = userId,
                Name = "Existing BH",
                AllowedGender = Gender.Mixed,
                Address = "123 Main St"
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

            Func<Task> act = async () => await _service.UpdateAsync(id, userId, new UpdateBoardingHouseDto
            {
                AllowedGender = "Unknown"
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid gender value*");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenDifferentOwner()
        {
            var id = Guid.NewGuid();
            var ownerA = Guid.NewGuid();
            var ownerB = Guid.NewGuid();
            var existing = new BoardingHouse
            {
                Id = id,
                UserId = ownerA,
                Name = "Existing BH",
                AllowedGender = Gender.Mixed,
                Address = "123 Main St"
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

            var result = await _service.UpdateAsync(id, ownerB, new UpdateBoardingHouseDto
            {
                Name = "Updated Name"
            });

            result.Should().BeNull();
            _repo.Verify(r => r.UpdateAsync(It.IsAny<BoardingHouse>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenOwnerAndExists()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new BoardingHouse
            {
                Id = id,
                UserId = userId,
                Name = "BH",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            });

            var result = await _service.DeleteAsync(id, userId);

            result.Should().BeTrue();
            _repo.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenDifferentOwner()
        {
            var id = Guid.NewGuid();
            var ownerA = Guid.NewGuid();
            var ownerB = Guid.NewGuid();
            _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new BoardingHouse
            {
                Id = id,
                UserId = ownerA,
                Name = "BH",
                AllowedGender = Gender.Mixed,
                Address = "Addr"
            });

            var result = await _service.DeleteAsync(id, ownerB);

            result.Should().BeFalse();
            _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((BoardingHouse?)null);

            var result = await _service.DeleteAsync(id, userId);

            result.Should().BeFalse();
            _repo.Verify(r => r.GetByIdAsync(id), Times.Once);
            _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }
    }
}
