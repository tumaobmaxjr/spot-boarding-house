using SpotBoardingHouse.Application.DTOs.Amenities;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepository;

        public AmenityService(IAmenityRepository amenityRepository)
        {
            _amenityRepository = amenityRepository;
        }

        public async Task<IEnumerable<AmenityDto>> GetAllAsync()
        {
            var amenities = await _amenityRepository.GetAllAsync();
            return amenities.Select(MapToDto);
        }

        public async Task<AmenityDto?> GetByIdAsync(Guid id)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);
            return amenity == null ? null : MapToDto(amenity);
        }

        public async Task<AmenityDto> CreateAsync(CreateAmenityDto createAmenityDto)
        {
            var name = createAmenityDto.Name.Trim();

            if (await _amenityRepository.NameExistsAsync(name))
            {
                throw new InvalidOperationException($"Amenity '{name}' already exists.");
            }

            var amenity = new Amenity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Icon = createAmenityDto.Icon?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _amenityRepository.AddAsync(amenity);

            return MapToDto(created);
        }

        public async Task<AmenityDto?> UpdateAsync(Guid id, UpdateAmenityDto updateAmenityDto)
        {
            var existing = await _amenityRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(updateAmenityDto.Name))
            {
                var newName = updateAmenityDto.Name.Trim();

                if (await _amenityRepository.NameExistsAsync(newName, id))
                {
                    throw new InvalidOperationException($"Amenity '{newName}' already exists.");
                }

                existing.Name = newName;
            }

            if (updateAmenityDto.Icon != null)
            {
                existing.Icon = updateAmenityDto.Icon.Trim();
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _amenityRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _amenityRepository.ExistsAsync(id))
            {
                return false;
            }

            await _amenityRepository.DeleteAsync(id);
            return true;
        }

        private static AmenityDto MapToDto(Amenity amenity)
        {
            return new AmenityDto
            {
                Id = amenity.Id,
                Name = amenity.Name,
                Icon = amenity.Icon,
                CreatedAt = amenity.CreatedAt,
                UpdatedAt = amenity.UpdatedAt
            };
        }
    }
}
