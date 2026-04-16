using FluentAssertions;
using Moq;
using SpotBoardingHouse.Application.DTOs.Contacts;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Services;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.Tests.Services
{
    public class ContactServiceTests
    {
        private readonly Mock<IContactRepository> _contactRepository = new();
        private readonly Mock<IBoardingHouseRepository> _boardingHouseRepository = new();
        private readonly ContactService _service;

        public ContactServiceTests()
        {
            _service = new ContactService(_contactRepository.Object, _boardingHouseRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenBoardingHouseDoesNotExist()
        {
            var boardingHouseId = Guid.NewGuid();
            var dto = new CreateContactDto
            {
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09171234567",
                Network = Network.Globe,
                IsPrimary = false
            };

            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(false);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Boarding house not found*");
        }

        [Fact]
        public async Task CreateAsync_ShouldTrimPhone_AndThrow_WhenDuplicateExists()
        {
            var boardingHouseId = Guid.NewGuid();
            var dto = new CreateContactDto
            {
                BoardingHouseId = boardingHouseId,
                PhoneNumber = " 09171234567 ",
                Network = Network.Globe,
                IsPrimary = false
            };

            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(true);
            _contactRepository.Setup(r => r.PhoneExistsAsync(boardingHouseId, "09171234567", null)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Phone number already exists*");
        }

        [Fact]
        public async Task CreateAsync_ShouldUnsetCurrentPrimary_WhenNewContactIsPrimary()
        {
            var boardingHouseId = Guid.NewGuid();
            var existingPrimary = new Contact
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09170000000",
                Network = Network.Smart,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(true);
            _contactRepository.Setup(r => r.PhoneExistsAsync(boardingHouseId, "09171234567", null)).ReturnsAsync(false);
            _contactRepository.Setup(r => r.GetPrimaryByBoardingHouseId(boardingHouseId, null)).ReturnsAsync(existingPrimary);
            _contactRepository.Setup(r => r.AddAsync(It.IsAny<Contact>())).ReturnsAsync((Contact c) => c);

            var dto = new CreateContactDto
            {
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09171234567",
                Network = Network.Globe,
                IsPrimary = true
            };

            var result = await _service.CreateAsync(dto);

            result.IsPrimary.Should().BeTrue();
            existingPrimary.IsPrimary.Should().BeFalse();
            _contactRepository.Verify(r => r.UpdateAsync(existingPrimary), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenContactNotFound()
        {
            var id = Guid.NewGuid();
            _contactRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Contact?)null);

            var result = await _service.UpdateAsync(id, new UpdateContactDto { PhoneNumber = "09171234567" });

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDuplicatePhoneExists()
        {
            var id = Guid.NewGuid();
            var boardingHouseId = Guid.NewGuid();
            var existing = new Contact
            {
                Id = id,
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09170000000",
                Network = Network.Smart,
                IsPrimary = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _contactRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _contactRepository.Setup(r => r.PhoneExistsAsync(boardingHouseId, "09171234567", id)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.UpdateAsync(id, new UpdateContactDto { PhoneNumber = "09171234567" });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Phone number already exists*");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUnsetCurrentPrimary_WhenSettingPrimaryToTrue()
        {
            var id = Guid.NewGuid();
            var boardingHouseId = Guid.NewGuid();
            var existing = new Contact
            {
                Id = id,
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09170000000",
                Network = Network.Smart,
                IsPrimary = false,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var currentPrimary = new Contact
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                PhoneNumber = "09179999999",
                Network = Network.Globe,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _contactRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _contactRepository.Setup(r => r.PhoneExistsAsync(boardingHouseId, "09170000000", id)).ReturnsAsync(false);
            _contactRepository.Setup(r => r.GetPrimaryByBoardingHouseId(boardingHouseId, id)).ReturnsAsync(currentPrimary);

            var result = await _service.UpdateAsync(id, new UpdateContactDto { IsPrimary = true });

            result.Should().NotBeNull();
            result!.IsPrimary.Should().BeTrue();
            currentPrimary.IsPrimary.Should().BeFalse();
            _contactRepository.Verify(r => r.UpdateAsync(currentPrimary), Times.Once);
            _contactRepository.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
        {
            var id = Guid.NewGuid();
            _contactRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

            var result = await _service.DeleteAsync(id);

            result.Should().BeFalse();
            _contactRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenContactExists()
        {
            var id = Guid.NewGuid();
            _contactRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);

            var result = await _service.DeleteAsync(id);

            result.Should().BeTrue();
            _contactRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }
    }
}
