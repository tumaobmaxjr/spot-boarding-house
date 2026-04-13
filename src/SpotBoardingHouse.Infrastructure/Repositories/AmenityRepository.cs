using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class AmenityRepository : IAmenityRepository
    {
        private readonly ApplicationDbContext _context;

        public AmenityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Amenity>> GetAllAsync()
        {
            return await _context.Amenities
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Amenity?> GetByIdAsync(Guid id)
        {
            return await _context.Amenities
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<Amenity> AddAsync(Amenity amenity)
        {
            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();
            return amenity;
        }

        public async Task UpdateAsync(Amenity amenity)
        {
            _context.Amenities.Update(amenity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var amenity = await _context.Amenities.FindAsync(id);
            if (amenity != null)
            {
                amenity.IsDeleted = true;
                amenity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Amenities.AnyAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludedId = null)
        {
            var query = _context.Amenities
                .Where(a => a.Name == name && !a.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(a => a.Id != excludedId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
