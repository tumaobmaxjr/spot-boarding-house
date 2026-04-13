using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class BoardingHouseAmenityRepository : IBoardingHouseAmenityRepository
    {
        private readonly ApplicationDbContext _context;

        public BoardingHouseAmenityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Amenity>> GetAmenitiesByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            return await _context.BoardingHouseAmenities
                .Where(x => x.BoardingHouseId == boardingHouseId)
                .Select(x => x.Amenity)
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid boardingHouseId, Guid amenityId)
        {
            return await _context.BoardingHouseAmenities
                .AnyAsync(x => x.BoardingHouseId == boardingHouseId && x.AmenityId == amenityId);
        }

        public async Task AddAsync(BoardingHouseAmenity boardingHouseAmenity)
        {
            _context.BoardingHouseAmenities.Add(boardingHouseAmenity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid boardingHouseId, Guid amenityId)
        {
            var boardingHouseAmenity = await _context.BoardingHouseAmenities
                .FirstOrDefaultAsync(x => x.BoardingHouseId == boardingHouseId && x.AmenityId == amenityId);

            if (boardingHouseAmenity != null)
            {
                _context.BoardingHouseAmenities.Remove(boardingHouseAmenity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
