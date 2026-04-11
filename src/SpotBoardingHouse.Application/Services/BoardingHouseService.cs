using SpotBoardingHouse.Application.DTOs.BoardingHouses;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.Services
{
    public class BoardingHouseService : IBoardingHouseService
    {
        private readonly IBoardingHouseRepository _boardingHouseRepository;

        public BoardingHouseService(IBoardingHouseRepository boardingHouseRepository)
        {
            _boardingHouseRepository = boardingHouseRepository;
        }

        public async Task<IEnumerable<BoardingHouseDto>> GetAllAsync()
        {
            var boardingHouses = await _boardingHouseRepository.GetAllAsync();
            return boardingHouses.Select(MapToDto);
        }

        public async Task<IEnumerable<BoardingHouseDto>> GetAllByUserIdAsync(Guid id)
        {
            var boardingHouses = await _boardingHouseRepository.GetAllByUserIdAsync(id);
            return boardingHouses.Select(MapToDto);
        }

        public async Task<BoardingHouseDto?> GetByIdAsync(Guid id)
        {
            var boardingHouse = await _boardingHouseRepository.GetByIdAsync(id);
            return boardingHouse == null ? null : MapToDto(boardingHouse);
        }

        public async Task<BoardingHouseDto> CreateAsync(Guid userId, CreateBoardingHouseDto creatingBoardingHouse)
        {
            if (await _boardingHouseRepository.NameExistsForUserAsync(creatingBoardingHouse.Name, userId))
            {
                throw new InvalidOperationException($"Boarding house with name '{creatingBoardingHouse.Name}' already exists.");
            }

            if (!Enum.TryParse<Gender>(creatingBoardingHouse.AllowedGender, true, out var gender))
            {
                throw new InvalidOperationException($"Invalid gender value: '{creatingBoardingHouse.AllowedGender}'. Must be Male, Female, or Mixed.");
            }

            var boardingHouse = new BoardingHouse
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = creatingBoardingHouse.Name.Trim(),
                Description = creatingBoardingHouse.Description?.Trim(),
                AllowedGender = gender,
                Address = creatingBoardingHouse.Address.Trim(),
                Latitude = creatingBoardingHouse.Latitude,
                Longitude = creatingBoardingHouse.Longitude,
                FacebookLink = creatingBoardingHouse.FacebookLink?.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _boardingHouseRepository.AddAsync(boardingHouse);

            return MapToDto(created);
        }

        public async Task<BoardingHouseDto?> UpdateAsync(Guid id, Guid userId, UpdateBoardingHouseDto updateBoardingHouseDto)
        {
            var existing = await _boardingHouseRepository.GetByIdAsync(id);
            if (existing == null || existing.UserId != userId)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(updateBoardingHouseDto.Name) &&
                await _boardingHouseRepository.NameExistsForUserAsync(updateBoardingHouseDto.Name, existing.UserId, id))
            {
                throw new InvalidOperationException($"Boarding house with name '{updateBoardingHouseDto.Name}' already exists.");
            }

            if (!string.IsNullOrWhiteSpace(updateBoardingHouseDto.Name))
                existing.Name = updateBoardingHouseDto.Name.Trim();

            if (updateBoardingHouseDto.Description != null)
                existing.Description = updateBoardingHouseDto.Description.Trim();

            if (!string.IsNullOrWhiteSpace(updateBoardingHouseDto.AllowedGender))
            {
                if (!Enum.TryParse<Gender>(updateBoardingHouseDto.AllowedGender, true, out var gender))
                {
                    throw new InvalidOperationException($"Invalid gender value: '{updateBoardingHouseDto.AllowedGender}'. Must be Male, Female, or Mixed.");
                }

                existing.AllowedGender = gender;
            }

            if (!string.IsNullOrWhiteSpace(updateBoardingHouseDto.Address))
                existing.Address = updateBoardingHouseDto.Address.Trim();

            if (updateBoardingHouseDto.Latitude.HasValue)
                existing.Latitude = updateBoardingHouseDto.Latitude.Value;

            if (updateBoardingHouseDto.Longitude.HasValue)
                existing.Longitude = updateBoardingHouseDto.Longitude.Value;

            if (updateBoardingHouseDto.FacebookLink != null)
                existing.FacebookLink = updateBoardingHouseDto.FacebookLink.Trim();

            existing.UpdatedAt = DateTime.UtcNow;

            await _boardingHouseRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            return await _boardingHouseRepository.SoftDeleteCascadeAsync(id, userId);
        }

        private static BoardingHouseDto MapToDto(BoardingHouse boardingHouse)
        {
            return new BoardingHouseDto
            {
                OwnerName = boardingHouse.User != null ? $"{boardingHouse.User.FirstName} {boardingHouse.User.LastName}" : null,

                Id = boardingHouse.Id,
                UserId = boardingHouse.UserId,
                Name = boardingHouse.Name,
                Description = boardingHouse.Description,
                AllowedGender = boardingHouse.AllowedGender.ToString(),
                Address = boardingHouse.Address,
                Latitude = boardingHouse.Latitude,
                Longitude = boardingHouse.Longitude,
                FacebookLink = boardingHouse.FacebookLink,
                CreatedAt = boardingHouse.CreatedAt,
                UpdatedAt = boardingHouse.UpdatedAt,
            };
        }
    }
}
