using SpotBoardingHouse.Application.DTOs.Rooms;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllAsync();
        Task<IEnumerable<RoomDto>> GetAllByUserIdAsync(Guid userId);
        Task<RoomDto?> GetByIdAsync(Guid id);
        Task<RoomDto> CreateAsync(Guid boardingHouseId, CreateRoomDto createRoomDto);
        Task<RoomDto?> UpdateAsync(Guid id, Guid boardingHouseId, UpdateRoomDto updateRoomDto);
        Task<bool> DeleteAsync(Guid id, Guid boardingHouseId);
    }
}
