using SpotBoardingHouse.Application.DTOs.RoomOffers;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IRoomOfferService
    {
        Task<IEnumerable<RoomOfferDto>> GetAllByRoomIdAsync(Guid roomId);
        Task<RoomOfferDto?> GetByIdAsync(Guid id);
        Task<RoomOfferDto> CreateAsync(CreateRoomOfferDto createRoomOfferDto);
        Task<RoomOfferDto?> UpdateAsync(Guid id, Guid roomId, UpdateRoomOfferDto updateRoomOfferDto);
        Task<bool> DeleteAsync(Guid id, Guid roomId);
    }
}
