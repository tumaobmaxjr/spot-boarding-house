using SpotBoardingHouse.Application.DTOs.BoardingHouses;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IBoardingHouseService
    {
        Task<IEnumerable<BoardingHouseDto>> GetAllByUserIdAsync(Guid id);
        Task<BoardingHouseDto?> GetByIdAsync(Guid id);
        Task<BoardingHouseDto> CreateAsync(Guid userId, CreateBoardingHouseDto creatingBoardingHouse);
        Task<BoardingHouseDto?> UpdateAsync(Guid id, Guid userId, UpdateBoardingHouseDto updateBoardingHouseDto);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
