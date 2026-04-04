using SpotBoardingHouse.Application.DTOs.RoomTypes;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IRoomTypeService
    {
        Task<IEnumerable<RoomTypeDto>> GetAllAsync();
        Task<RoomTypeDto?> GetByIdAsync(Guid id);
        Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto createRoomType);
        Task<RoomTypeDto?> UpdateAsync(Guid id, UpdateRoomTypeDto updateRoomTypeDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
