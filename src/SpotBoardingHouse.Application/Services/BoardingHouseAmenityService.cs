using SpotBoardingHouse.Application.DTOs.Amenities;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class BoardingHouseAmenityService : IBoardingHouseAmenityService
    {
        private readonly IBoardingHouseAmenityRepository _boardingHouseAmenityRepository;
        private readonly IBoardingHouseRepository _boardingHouseRepository;
        private readonly IAmenityRepository _amenityRepository;

        public BoardingHouseAmenityService(
            IBoardingHouseAmenityRepository boardingHouseAmenityRepository,
            IBoardingHouseRepository boardingHouseRepository,
            IAmenityRepository amenityRepository)
        {
            _boardingHouseAmenityRepository = boardingHouseAmenityRepository;
            _boardingHouseRepository = boardingHouseRepository;
            _amenityRepository = amenityRepository;
        }

        public async Task<IEnumerable<AmenityDto>> GetAmenitiesByBoardingHouseAsync(Guid boardingHouseId)
        {
            var amenities = await _boardingHouseAmenityRepository.GetAmenitiesByBoardingHouseIdAsync(boardingHouseId);
            return amenities.Select(MapToDto);
        }

        public async Task AttachAsync(Guid boardingHouseId, Guid amenityId)
        {
            if (!await _boardingHouseRepository.ExistsAsync(boardingHouseId))
            {
                throw new InvalidOperationException("Boarding house not found.");
            }

            if (!await _amenityRepository.ExistsAsync(amenityId))
            {
                throw new InvalidOperationException("Amenity not found.");
            }

            if (await _boardingHouseAmenityRepository.ExistsAsync(boardingHouseId, amenityId))
            {
                throw new InvalidOperationException("Amenity is already attached to this boarding house.");
            }

            await _boardingHouseAmenityRepository.AddAsync(new BoardingHouseAmenity
            {
                BoardingHouseId = boardingHouseId,
                AmenityId = amenityId
            });
        }

        public async Task<bool> DetachAsync(Guid boardingHouseId, Guid amenityId)
        {
            if (!await _boardingHouseAmenityRepository.ExistsAsync(boardingHouseId, amenityId))
            {
                return false;
            }

            await _boardingHouseAmenityRepository.DeleteAsync(boardingHouseId, amenityId);

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
