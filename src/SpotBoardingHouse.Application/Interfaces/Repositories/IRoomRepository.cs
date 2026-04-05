using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<IEnumerable<Room>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<Room>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId);
        Task<Room?> GetByIdAsync(Guid id);
        Task<Room> AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> RoomNumberExistsAsync(string roomNumber, Guid boardingHouseId, Guid? excludeId = null);
    }
}
