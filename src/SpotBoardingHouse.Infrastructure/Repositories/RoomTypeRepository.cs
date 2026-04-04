using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entity;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomType>> GetAllAsync()
        {
            return await _context.RoomTypes
                .Where(rt => !rt.IsDeleted)
                .OrderBy(rt => rt.Name)
                .ToListAsync();
        }

        public async Task<RoomType?> GetByIdAsync(Guid id)
        {
            return await _context.RoomTypes
                .FirstOrDefaultAsync(rt => rt.Id == id && !rt.IsDeleted);
        }

        public async Task<RoomType> AddAsync(RoomType roomType)
        {
            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();
            return roomType;
        }

        public async Task UpdateAsync(RoomType roomType)
        {
            _context.RoomTypes.Update(roomType);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.RoomTypes.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.RoomTypes.AnyAsync(rt => rt.Id == id && !rt.IsDeleted);
        }

        public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
        {
            var query = _context.RoomTypes.Where(rt => rt.Name == name && !rt.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(rt => rt.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
