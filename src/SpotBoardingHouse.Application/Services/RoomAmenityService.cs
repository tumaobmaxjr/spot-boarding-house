using SpotBoardingHouse.Application.DTOs.Amenities;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class RoomAmenityService : IRoomAmenityService
    {
        private readonly IRoomAmenityRepository _roomAmenityRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IAmenityRepository _amenityRepository;

        public RoomAmenityService(
            IRoomAmenityRepository roomAmenityRepository,
            IRoomRepository roomRepository,
            IAmenityRepository amenityRepository)
        {
            _roomAmenityRepository = roomAmenityRepository;
            _roomRepository = roomRepository;
            _amenityRepository = amenityRepository;
        }

        public async Task<IEnumerable<AmenityDto>> GetAmenitiesByRoomAsync(Guid roomId)
        {
            var amenities = await _roomAmenityRepository.GetAmenitiesByRoomIdAsync(roomId);
            return amenities.Select(MapToDto);
        }

        public async Task AttachAsync(Guid roomId, Guid amenityId)
        {
            if (!await _roomRepository.ExistsAsync(roomId))
            {
                throw new InvalidOperationException("Room not found.");
            }

            if (!await _amenityRepository.ExistsAsync(amenityId))
            {
                throw new InvalidOperationException("Amenity not found.");
            }

            if (await _roomAmenityRepository.ExistsAsync(roomId, amenityId))
            {
                throw new InvalidOperationException("Amenity is already attached to this room.");
            }

            await _roomAmenityRepository.AddAsync(new RoomAmenity
            {
                RoomId = roomId,
                AmenityId = amenityId
            });
        }

        public async Task<bool> DetachAsync(Guid roomId, Guid amenityId)
        {
            if (!await _roomAmenityRepository.ExistsAsync(roomId, amenityId))
            {
                return false;
            }

            await _roomAmenityRepository.DeleteAsync(roomId, amenityId);

            return true;
        }

        private static AmenityDto MapToDto(Amenity amenity)
        {
            return new AmenityDto
            {
                Id = amenity.Id,
                Name = amenity.Name,
                Icon = amenity.Icon,
                CreatedAt = amenity.CreatedAt,
                UpdatedAt = amenity.UpdatedAt
            };
        }
    }
}
