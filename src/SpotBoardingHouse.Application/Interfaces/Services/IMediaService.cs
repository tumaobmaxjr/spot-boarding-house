using SpotBoardingHouse.Application.DTOs.Media;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IMediaService
    {
        Task<IEnumerable<MediaDto>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId);
        Task<IEnumerable<MediaDto>> GetAllByRoomIdAsync(Guid roomId);
        Task<MediaDto?> GetByIdAsync(Guid id);
        Task<MediaDto> CreateAsync(CreateMediaDto createMediaDto);
        Task<MediaDto?> UpdateAsync(Guid id, UpdateMediaDto updateMediaDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
