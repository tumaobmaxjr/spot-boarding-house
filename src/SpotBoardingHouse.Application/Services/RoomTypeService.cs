using SpotBoardingHouse.Application.DTOs.RoomTypes;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _roomTypeRepository;

        public RoomTypeService(IRoomTypeRepository roomTypeRepository)
        {
            _roomTypeRepository = roomTypeRepository;
        }

        public async Task<IEnumerable<RoomTypeDto>> GetAllAsync()
        {
            var items = await _roomTypeRepository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<RoomTypeDto?> GetByIdAsync(Guid id)
        {
            var item = await _roomTypeRepository.GetByIdAsync(id);
            return item == null ? null : MapToDto(item);
        }

        public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto createRoomType)
        {
            if (await _roomTypeRepository.NameExistsAsync(createRoomType.Name))
            {
                throw new InvalidOperationException($"Room type '{createRoomType.Name}' already exists.");
            }

            var roomType = new RoomType
            {
                Id = Guid.NewGuid(),
                Name = createRoomType.Name.Trim(),
                Description = createRoomType.Description?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _roomTypeRepository.AddAsync(roomType);

            return MapToDto(created);
        }

        public async Task<RoomTypeDto?> UpdateAsync(Guid id, UpdateRoomTypeDto updateRoomTypeDto)
        {
            var existing = await _roomTypeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(updateRoomTypeDto.Name))
            {
                if (await _roomTypeRepository.NameExistsAsync(updateRoomTypeDto.Name, id))
                {
                    throw new InvalidOperationException($"Room type '{updateRoomTypeDto.Name}' already exists.'");
                }
                existing.Name = updateRoomTypeDto.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(updateRoomTypeDto.Description))
            {
                existing.Description = updateRoomTypeDto.Description.Trim();
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _roomTypeRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var exists = await _roomTypeRepository.ExistsAsync(id);
            if (!exists)
            {
                return false;
            }

            await _roomTypeRepository.DeleteAsync(id);

            return true;
        }

        private static RoomTypeDto MapToDto(RoomType roomType)
        {
            return new RoomTypeDto
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Description = roomType.Description,
                CreatedAt = roomType.CreatedAt,
                UpdatedAt = roomType.UpdatedAt
            };
        }
    }
}
