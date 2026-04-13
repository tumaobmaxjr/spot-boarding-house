using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IRoomAmenityRepository
    {
        Task<IEnumerable<Amenity>> GetAmenitiesByRoomIdAsync(Guid roomId);
        Task<bool> ExistsAsync(Guid roomId, Guid amenityId);
        Task AddAsync(RoomAmenity roomAmenity);
        Task DeleteAsync(Guid roomId, Guid amenityId);
    }
}
