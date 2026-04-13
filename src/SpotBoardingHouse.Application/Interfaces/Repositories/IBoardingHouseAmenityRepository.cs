using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IBoardingHouseAmenityRepository
    {
        Task<IEnumerable<Amenity>> GetAmenitiesByBoardingHouseIdAsync(Guid boardingHouseId);
        Task<bool> ExistsAsync(Guid boardingHouseId, Guid amenityId);
        Task AddAsync(BoardingHouseAmenity boardingHouseAmenity);
        Task DeleteAsync(Guid boardingHouseId, Guid amenityId);
    }
}
