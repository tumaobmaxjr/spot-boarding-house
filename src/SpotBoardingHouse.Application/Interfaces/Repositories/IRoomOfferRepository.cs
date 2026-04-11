using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IRoomOfferRepository
    {
        Task<IEnumerable<RoomOffer>> GetAllByRoomIdAsync(Guid roomId);
        Task<RoomOffer?> GetByIdAsync(Guid id);
        Task<RoomOffer> AddAsync(RoomOffer roomOffer);
        Task UpdateAsync(RoomOffer roomOffer);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> TypeExistsForRoomAsync(Guid roomId, Guid roomTypeId, Guid? excludedId = null);
    }
}
