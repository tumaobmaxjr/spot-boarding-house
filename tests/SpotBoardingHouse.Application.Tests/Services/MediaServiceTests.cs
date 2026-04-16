using FluentAssertions;
using Moq;
using SpotBoardingHouse.Application.DTOs.Media;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Tests.Services
{
    public class MediaServiceTests
    {
        private readonly Mock<IMediaRepository> _mediaRepository = new();
        private readonly Mock<IBoardingHouseRepository> _boardingHouseRepository = new();
        private readonly Mock<IRoomRepository> _roomRepository = new();
        private readonly MediaService _service;

        public MediaServiceTests()
        {
            _service = new MediaService(
                _mediaRepository.Object,
                _boardingHouseRepository.Object,
                _roomRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenTargetIsInvalidXor()
        {
            var dtoWithNoTarget = new CreateMediaDto
            {
                Url = "https://example.com/image.jpg",
                DisplayOrder = 1
            };

            var dtoWithBothTargets = new CreateMediaDto
            {
                BoardingHouseId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                Url = "https://example.com/image.jpg",
                DisplayOrder = 1
            };

            Func<Task> actNoTarget = async () => await _service.CreateAsync(dtoWithNoTarget);
            Func<Task> actBothTargets = async () => await _service.CreateAsync(dtoWithBothTargets);

            await actNoTarget.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Exactly one target must be set*");
            await actBothTargets.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Exactly one target must be set*");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenUrlIsEmptyAfterTrim()
        {
            var dto = new CreateMediaDto
            {
                BoardingHouseId = Guid.NewGuid(),
                Url = "   ",
                DisplayOrder = 1
            };

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Media URL is required*");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenBoardingHouseDoesNotExist()
        {
            var boardingHouseId = Guid.NewGuid();
            var dto = new CreateMediaDto
            {
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/image.jpg",
                DisplayOrder = 1
            };

            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(false);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Boarding house not found*");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenRoomDoesNotExist()
        {
            var roomId = Guid.NewGuid();
            var dto = new CreateMediaDto
            {
                RoomId = roomId,
                Url = "https://example.com/image.jpg",
                DisplayOrder = 1
            };

            _roomRepository.Setup(r => r.ExistsAsync(roomId)).ReturnsAsync(false);

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Room not found*");
        }

        [Fact]
        public async Task CreateAsync_ShouldUnsetCurrentPrimary_WhenBoardingHouseMediaIsPrimary()
        {
            var boardingHouseId = Guid.NewGuid();
            var currentPrimary = new Media
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/current-primary.jpg",
                IsPrimary = true,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(true);
            _mediaRepository.Setup(r => r.GetPrimaryByBoardingHouseIdAsync(boardingHouseId, null)).ReturnsAsync(currentPrimary);
            _mediaRepository.Setup(r => r.AddAsync(It.IsAny<Media>())).ReturnsAsync((Media m) => m);

            var dto = new CreateMediaDto
            {
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/new-primary.jpg",
                IsPrimary = true,
                DisplayOrder = 1
            };

            var result = await _service.CreateAsync(dto);

            result.IsPrimary.Should().BeTrue();
            currentPrimary.IsPrimary.Should().BeFalse();
            _mediaRepository.Verify(r => r.UpdateAsync(currentPrimary), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenMediaNotFound()
        {
            var id = Guid.NewGuid();
            _mediaRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Media?)null);

            var result = await _service.UpdateAsync(id, new UpdateMediaDto { Url = "https://example.com/updated.jpg" });

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenUrlIsEmptyAfterTrim()
        {
            var id = Guid.NewGuid();
            var boardingHouseId = Guid.NewGuid();
            var existing = new Media
            {
                Id = id,
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/original.jpg",
                IsPrimary = false,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _mediaRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.UpdateAsync(id, new UpdateMediaDto { Url = "   " });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Media URL cannot be empty*");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUnsetCurrentPrimary_WhenSettingPrimaryToTrue()
        {
            var id = Guid.NewGuid();
            var boardingHouseId = Guid.NewGuid();
            var existing = new Media
            {
                Id = id,
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/original.jpg",
                IsPrimary = false,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var currentPrimary = new Media
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                Url = "https://example.com/current-primary.jpg",
                IsPrimary = true,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _mediaRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _boardingHouseRepository.Setup(r => r.ExistsAsync(boardingHouseId)).ReturnsAsync(true);
            _mediaRepository.Setup(r => r.GetPrimaryByBoardingHouseIdAsync(boardingHouseId, id)).ReturnsAsync(currentPrimary);

            var result = await _service.UpdateAsync(id, new UpdateMediaDto { IsPrimary = true });

            result.Should().NotBeNull();
            result!.IsPrimary.Should().BeTrue();
            currentPrimary.IsPrimary.Should().BeFalse();
            _mediaRepository.Verify(r => r.UpdateAsync(currentPrimary), Times.Once);
            _mediaRepository.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenMediaNotFound()
        {
            var id = Guid.NewGuid();
            _mediaRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

            var result = await _service.DeleteAsync(id);

            result.Should().BeFalse();
            _mediaRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnTrue_WhenMediaExists()
        {
            var id = Guid.NewGuid();
            _mediaRepository.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);

            var result = await _service.DeleteAsync(id);

            result.Should().BeTrue();
            _mediaRepository.Verify(r => r.DeleteAsync(id), Times.Once);
        }
    }
}
