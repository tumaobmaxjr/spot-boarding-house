using SpotBoardingHouse.Application.DTOs.Amenities;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IAmenityService
    {
        Task<IEnumerable<AmenityDto>> GetAllAsync();
        Task<AmenityDto?> GetByIdAsync(Guid id);
        Task<AmenityDto> CreateAsync(CreateAmenityDto createAmenityDto);
        Task<AmenityDto?> UpdateAsync(Guid id, UpdateAmenityDto updateAmenityDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
