using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IMediaRepository
    {
        Task<IEnumerable<Media>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId);
        Task<IEnumerable<Media>> GetAllByRoomIdAsync(Guid roomId);
        Task<Media?> GetByIdAsync(Guid id);
        Task<Media> AddAsync(Media media);
        Task UpdateAsync(Media media);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<Media?> GetPrimaryByBoardingHouseIdAsync(Guid boardingHouseId, Guid? excludedId = null);
        Task<Media?> GetPrimaryByRoomIdAsync(Guid roomId, Guid? excludedId = null);
    }
}
