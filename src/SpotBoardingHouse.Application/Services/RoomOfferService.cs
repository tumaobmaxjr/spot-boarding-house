using SpotBoardingHouse.Application.DTOs.RoomOffers;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.Services
{
    public class RoomOfferService : IRoomOfferService
    {
        private readonly IRoomOfferRepository _roomOfferRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;

        public RoomOfferService(
            IRoomOfferRepository roomOfferRepository,
            IRoomRepository roomRepository,
            IRoomTypeRepository roomTypeRepository)
        {
            _roomOfferRepository = roomOfferRepository;
            _roomRepository = roomRepository;
            _roomTypeRepository = roomTypeRepository;
        }

        public async Task<IEnumerable<RoomOfferDto>> GetAllByRoomIdAsync(Guid roomId)
        {
            var roomOffers = await _roomOfferRepository.GetAllByRoomIdAsync(roomId);
            return roomOffers.Select(MapToDto);
        }

        public async Task<RoomOfferDto?> GetByIdAsync(Guid id)
        {
            var roomOffer = await _roomOfferRepository.GetByIdAsync(id);
            return roomOffer == null ? null : MapToDto(roomOffer);
        }

        public async Task<RoomOfferDto> CreateAsync(CreateRoomOfferDto createRoomOfferDto)
        {
            var room = await _roomRepository.GetByIdAsync(createRoomOfferDto.RoomId);
            if (room == null)
            {
                throw new InvalidOperationException($"Room not found.");
            }

            var roomType = await _roomTypeRepository.GetByIdAsync(createRoomOfferDto.RoomTypeId);
            if (roomType == null)
            {
                throw new InvalidOperationException($"Room type not found.");
            }

            if (createRoomOfferDto.Capacity > room.BaseCapacity)
            {
                throw new InvalidOperationException($"Offer capacity ({createRoomOfferDto.Capacity}) cannot exceed room base capacity ({room.BaseCapacity}).");
            }

            if (createRoomOfferDto.Capacity < roomType.MinOccupants || createRoomOfferDto.Capacity > roomType.MaxOccupants)
            {
                throw new InvalidOperationException($"Capacity {createRoomOfferDto.Capacity} is invalid for '{roomType.Name}'. Allowed: {roomType.MinOccupants} to {roomType.MaxOccupants}.");
            }

            if (createRoomOfferDto.MaxPrice < createRoomOfferDto.MinPrice)
            {
                throw new InvalidOperationException("Max price must be greater than or equal to min price.");
            }

            if (await _roomOfferRepository.TypeExistsForRoomAsync(createRoomOfferDto.RoomId, createRoomOfferDto.RoomTypeId))
            {
                throw new InvalidOperationException("This room type is already configured for this room.");
            }

            var roomOffer = new RoomOffer
            {
                Id = Guid.NewGuid(),
                RoomId = createRoomOfferDto.RoomId,
                RoomTypeId = createRoomOfferDto.RoomTypeId,
                Capacity = createRoomOfferDto.Capacity,
                MinPrice = createRoomOfferDto.MinPrice,
                MaxPrice = createRoomOfferDto.MaxPrice,
                IsAvailable = createRoomOfferDto.IsAvailable,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _roomOfferRepository.AddAsync(roomOffer);

            return MapToDto(created);
        }

        public async Task<RoomOfferDto?> UpdateAsync(Guid id, Guid roomId, UpdateRoomOfferDto updateRoomOfferDto)
        {
            var existing = await _roomOfferRepository.GetByIdAsync(id);
            if (existing == null || existing.RoomId != roomId)
            {
                return null;
            }

            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
            {
                return null;
            }

            if (updateRoomOfferDto.RoomTypeId.HasValue && await _roomOfferRepository.TypeExistsForRoomAsync(roomId, updateRoomOfferDto.RoomTypeId.Value, id))
            {
                throw new InvalidOperationException($"This room type is already configured for this room.");
            }

            var newRoomTypeId = updateRoomOfferDto.RoomTypeId ?? existing.RoomTypeId;
            var newCapacity = updateRoomOfferDto.Capacity ?? existing.Capacity;

            var roomType = await _roomTypeRepository.GetByIdAsync(newRoomTypeId);
            if (roomType == null)
            {
                return null;
            }

            if (newCapacity > room.BaseCapacity)
            {
                throw new InvalidOperationException($"Offer capacity ({newCapacity}) cannot exceed room base capacity ({room.BaseCapacity}).");
            }

            if (newCapacity < roomType.MinOccupants || newCapacity > roomType.MaxOccupants)
            {
                throw new InvalidOperationException($"Capacity {newCapacity} is invalid for '{roomType.Name}'. Allowed: {roomType.MinOccupants} to {roomType.MaxOccupants}.");
            }

            var newMin = updateRoomOfferDto.MinPrice ?? existing.MinPrice;
            var newMax = updateRoomOfferDto.MaxPrice ?? existing.MaxPrice;
            if (newMax < newMin)
            {
                throw new InvalidOperationException($"Max price must be greater than or equal to min price.");
            }

            if (updateRoomOfferDto.RoomTypeId.HasValue)
            {
                existing.RoomTypeId = updateRoomOfferDto.RoomTypeId.Value;
            }

            if (updateRoomOfferDto.Capacity.HasValue)
            {
                existing.Capacity = updateRoomOfferDto.Capacity.Value;
            }

            if (updateRoomOfferDto.MinPrice.HasValue)
            {
                existing.MinPrice = updateRoomOfferDto.MinPrice.Value;
            }

            if (updateRoomOfferDto.MaxPrice.HasValue)
            {
                existing.MaxPrice = updateRoomOfferDto.MaxPrice.Value;
            }

            if (updateRoomOfferDto.IsAvailable.HasValue)
            {
                existing.IsAvailable = updateRoomOfferDto.IsAvailable.Value;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _roomOfferRepository.UpdateAsync(existing);
            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid roomId)
        {
            var existing = await _roomOfferRepository.GetByIdAsync(id);
            if (existing == null || existing.RoomId != roomId)
                return false;

            await _roomOfferRepository.DeleteAsync(id);
            return true;
        }

        private static RoomOfferStatus GetOfferStatus(bool isAvailable)
        {
            return isAvailable ? RoomOfferStatus.Available : RoomOfferStatus.Unavailable;
        }

        private static RoomOfferDto MapToDto(RoomOffer roomOffer)
        {
            var offerStatus = GetOfferStatus(roomOffer.IsAvailable);

            return new RoomOfferDto
            {
                Id = roomOffer.Id,
                RoomId = roomOffer.RoomId,
                RoomTypeId = roomOffer.RoomTypeId,
                RoomTypeName = roomOffer.RoomType?.Name ?? string.Empty,
                Capacity = roomOffer.Capacity,
                MinPrice = roomOffer.MinPrice,
                MaxPrice = roomOffer.MaxPrice,
                IsAvailable = roomOffer.IsAvailable,
                OfferStatus = offerStatus.ToString(),
                CreatedAt = roomOffer.CreatedAt,
                UpdatedAt = roomOffer.UpdatedAt
            };
        }
    }
}
