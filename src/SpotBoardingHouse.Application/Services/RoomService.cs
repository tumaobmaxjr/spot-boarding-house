using SpotBoardingHouse.Application.DTOs.Rooms;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<RoomDto>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return rooms.Select(MapToDto);
        }

        public async Task<IEnumerable<RoomDto>> GetAllByUserIdAsync(Guid userId)
        {
            var rooms = await _roomRepository.GetAllByUserIdAsync(userId);
            return rooms.Select(MapToDto);
        }

        public async Task<IEnumerable<RoomDto>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            var rooms = await _roomRepository.GetAllByBoardingHouseIdAsync(boardingHouseId);
            return rooms.Select(MapToDto);
        }

        public async Task<RoomDto?> GetByIdAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            return room == null ? null : MapToDto(room);
        }

        public async Task<RoomDto> CreateAsync(Guid boardingHouseId, CreateRoomDto createRoomDto)
        {
            if (await _roomRepository.RoomNumberExistsAsync(createRoomDto.RoomNumber, boardingHouseId))
            {
                throw new InvalidOperationException($"Room '{createRoomDto.RoomNumber}' already exists in this boarding house.");
            }

            if (createRoomDto.OccupiedSlots > createRoomDto.BaseCapacity)
            {
                throw new InvalidOperationException("Occupied slots cannot exceed capacity.");
            }

            var room = new Room
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = boardingHouseId,
                RoomNumber = createRoomDto.RoomNumber.Trim(),
                BaseCapacity = createRoomDto.BaseCapacity,
                OccupiedSlots = createRoomDto.OccupiedSlots,
                AdditionalDescription = createRoomDto.AdditionalDescription?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _roomRepository.AddAsync(room);

            return MapToDto(created);
        }

        public async Task<RoomDto?> UpdateAsync(Guid id, Guid boardingHouseId, UpdateRoomDto updateRoomDto)
        {
            var existing = await _roomRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(updateRoomDto.RoomNumber) &&
                await _roomRepository.RoomNumberExistsAsync(updateRoomDto.RoomNumber, boardingHouseId, id))
            {
                throw new InvalidOperationException($"Room '{updateRoomDto.RoomNumber}' already exists in this boarding house.");
            }

            var newCapacity = updateRoomDto.BaseCapacity ?? existing.BaseCapacity;
            var newOccupied = updateRoomDto.OccupiedSlots ?? existing.OccupiedSlots;
            if (newOccupied > newCapacity)
            {
                throw new InvalidOperationException("Occupied slots cannot exceed capacity.");
            }

            if (!string.IsNullOrWhiteSpace(updateRoomDto.RoomNumber))
            {
                existing.RoomNumber = updateRoomDto.RoomNumber.Trim();
            }

            if (updateRoomDto.BaseCapacity.HasValue)
            {
                existing.BaseCapacity = updateRoomDto.BaseCapacity.Value;
            }

            if (updateRoomDto.OccupiedSlots.HasValue)
            {
                existing.OccupiedSlots = updateRoomDto.OccupiedSlots.Value;
            }

            if (updateRoomDto.AdditionalDescription != null)
            {
                existing.AdditionalDescription = updateRoomDto.AdditionalDescription.Trim();
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _roomRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid boardingHouseId)
        {
            var existing = await _roomRepository.GetByIdAsync(id);
            if (existing == null || existing.BoardingHouseId != boardingHouseId)
            {
                return false;
            }

            await _roomRepository.DeleteAsync(id);

            return true;
        }

        private static RoomOccupancyStatus GetOccupancyStatus(int occupiedSlots, int baseCapacity)
        {
            if (occupiedSlots <= 0) return RoomOccupancyStatus.Available;
            if (occupiedSlots >= baseCapacity) return RoomOccupancyStatus.Full;
            return RoomOccupancyStatus.PartiallyOccupied;
        }

        private static RoomDto MapToDto(Room room)
        {
            var remainingSlots = room.BaseCapacity - room.OccupiedSlots;
            if (remainingSlots < 0) remainingSlots = 0;

            var occupancyStatus = GetOccupancyStatus(room.OccupiedSlots, room.BaseCapacity);

            return new RoomDto
            {
                Id = room.Id,
                BoardingHouseId = room.BoardingHouseId,
                BoardingHouseName = room.BoardingHouse?.Name ?? string.Empty,
                RoomNumber = room.RoomNumber,
                BaseCapacity = room.BaseCapacity,
                OccupiedSlots = room.OccupiedSlots,
                RemainingSlots = remainingSlots,
                OccupancyStatus = occupancyStatus.ToString(),
                AdditionalDescription = room.AdditionalDescription,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }
    }
}
