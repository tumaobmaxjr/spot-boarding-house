using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IBoardingHouseRepository
    {
        Task<IEnumerable<BoardingHouse>> GetAllAsync();
        Task<IEnumerable<BoardingHouse>> GetAllByUserIdAsync(Guid userId);
        Task<BoardingHouse?> GetByIdAsync(Guid id);
        Task<BoardingHouse> AddAsync(BoardingHouse boardingHouse);
        Task UpdateAsync(BoardingHouse boardingHouse);
        Task<bool> SoftDeleteCascadeAsync(Guid id, Guid userId);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> NameExistsForUserAsync(string name, Guid userId, Guid? excludedId = null);
    }
}
