using SpotBoardingHouse.Application.DTOs.Media;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IBoardingHouseRepository _boardingHouseRepository;
        private readonly IRoomRepository _roomRepository;

        public MediaService(
            IMediaRepository mediaRepository,
            IBoardingHouseRepository boardingHouseRepository,
            IRoomRepository roomRepository)
        {
            _mediaRepository = mediaRepository;
            _boardingHouseRepository = boardingHouseRepository;
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<MediaDto>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            var media = await _mediaRepository.GetAllByBoardingHouseIdAsync(boardingHouseId);
            return media.Select(MapToDto);
        }

        public async Task<IEnumerable<MediaDto>> GetAllByRoomIdAsync(Guid roomId)
        {
            var media = await _mediaRepository.GetAllByRoomIdAsync(roomId);
            return media.Select(MapToDto);
        }

        public async Task<MediaDto?> GetByIdAsync(Guid id)
        {
            var media = await _mediaRepository.GetByIdAsync(id);
            return media == null ? null : MapToDto(media);
        }

        public async Task<MediaDto> CreateAsync(CreateMediaDto createMediaDto)
        {
            // XOR: exactly one target must be set
            if (!HasExactlyOneTarget(createMediaDto.BoardingHouseId, createMediaDto.RoomId))
            {
                throw new InvalidOperationException("Exactly one target must be set: BoardingHouseId or RoomId");
            }

            if (string.IsNullOrWhiteSpace(createMediaDto.Url))
            {
                throw new InvalidOperationException("Media URL is required");
            }

            if (createMediaDto.BoardingHouseId.HasValue && !await _boardingHouseRepository.ExistsAsync(createMediaDto.BoardingHouseId.Value))
            {
                throw new InvalidOperationException("Boarding house not found.");
            }

            if (createMediaDto.RoomId.HasValue && !await _roomRepository.ExistsAsync(createMediaDto.RoomId.Value))
            {
                throw new InvalidOperationException("Room not found.");
            }

            if (createMediaDto.IsPrimary)
            {
                await UnsetCurrentPrimaryAsync(
                    createMediaDto.BoardingHouseId,
                    createMediaDto.RoomId,
                    excludedId: null);
            }

            var media = new Media
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = createMediaDto.BoardingHouseId,
                RoomId = createMediaDto.RoomId,
                Url = createMediaDto.Url.Trim(),
                IsPrimary = createMediaDto.IsPrimary,
                DisplayOrder = createMediaDto.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _mediaRepository.AddAsync(media);

            return MapToDto(created);
        }

        public async Task<MediaDto?> UpdateAsync(Guid id, UpdateMediaDto updateMediaDto)
        {
            var existing = await _mediaRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            // Target is immutable on update
            // Validate existing target still points to an active resource.
            if (existing.BoardingHouseId.HasValue && !await _boardingHouseRepository.ExistsAsync(existing.BoardingHouseId.Value))
            {
                throw new InvalidOperationException("Boarding house not found.");
            }

            if (existing.RoomId.HasValue && !await _roomRepository.ExistsAsync(existing.RoomId.Value))
            {
                throw new InvalidOperationException("Room not found.");
            }

            var willBePrimary = updateMediaDto.IsPrimary ?? existing.IsPrimary;
            if (willBePrimary)
            {
                await UnsetCurrentPrimaryAsync(
                    existing.BoardingHouseId,
                    existing.RoomId,
                    excludedId: id);
            }

            if (updateMediaDto.Url != null)
            {
                var trimmedUrl = updateMediaDto.Url.Trim();
                if (string.IsNullOrWhiteSpace(trimmedUrl))
                {
                    throw new InvalidOperationException("Media URL cannot be empty.");
                }

                existing.Url = trimmedUrl;
            }

            if (updateMediaDto.IsPrimary.HasValue)
            {
                existing.IsPrimary = updateMediaDto.IsPrimary.Value;
            }

            if (updateMediaDto.DisplayOrder.HasValue)
            {
                existing.DisplayOrder = updateMediaDto.DisplayOrder.Value;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _mediaRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _mediaRepository.ExistsAsync(id))
            {
                return false;
            }

            await _mediaRepository.DeleteAsync(id);

            return true;
        }

        private async Task UnsetCurrentPrimaryAsync(Guid? boardingHouseId, Guid? roomId, Guid? excludedId)
        {
            if (boardingHouseId.HasValue)
            {
                var currentPrimary = await _mediaRepository.GetPrimaryByBoardingHouseIdAsync(boardingHouseId.Value, excludedId);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    await _mediaRepository.UpdateAsync(currentPrimary);
                }
            }

            if (roomId.HasValue)
            {
                var currentPrimary = await _mediaRepository.GetPrimaryByRoomIdAsync(roomId.Value, excludedId);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    await _mediaRepository.UpdateAsync(currentPrimary);
                }
            }
        }

        private static bool HasExactlyOneTarget(Guid? boardingHouseId, Guid? roomId)
        {
            return (boardingHouseId.HasValue && !roomId.HasValue) ||
                (!boardingHouseId.HasValue && roomId.HasValue);
        }

        private static MediaDto MapToDto(Media media)
        {
            return new MediaDto
            {
                Id = media.Id,
                BoardingHouseId = media.BoardingHouseId,
                RoomId = media.RoomId,
                Url = media.Url,
                IsPrimary = media.IsPrimary,
                DisplayOrder = media.DisplayOrder,
                CreatedAt = media.CreatedAt,
                UpdatedAt = media.UpdatedAt,
            };
        }
    }
}
