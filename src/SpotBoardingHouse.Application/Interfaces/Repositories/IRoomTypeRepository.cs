using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IRoomTypeRepository
    {
        Task<IEnumerable<RoomType>> GetAllAsync();
        Task<RoomType?> GetByIdAsync(Guid id);
        Task<RoomType> AddAsync(RoomType roomType);
        Task UpdateAsync(RoomType roomType);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    }
}
