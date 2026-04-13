using SpotBoardingHouse.Application.DTOs.Amenities;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IRoomAmenityService
    {
        Task<IEnumerable<AmenityDto>> GetAmenitiesByRoomAsync(Guid roomId);
        Task AttachAsync(Guid roomId, Guid amenityId);
        Task<bool> DetachAsync(Guid roomId, Guid amenityId);
    }
}
