using SpotBoardingHouse.Application.DTOs.Amenities;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IBoardingHouseAmenityService
    {
        Task<IEnumerable<AmenityDto>> GetAmenitiesByBoardingHouseAsync(Guid boardingHouseId);
        Task AttachAsync(Guid boardingHouseId, Guid amenityId);
        Task<bool> DetachAsync(Guid boardingHouseId, Guid amenityId);
    }
}
