using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly ApplicationDbContext _context;

        public MediaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Media>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            return await _context.Media
                .Where(m => m.BoardingHouseId == boardingHouseId && !m.IsDeleted)
                .OrderByDescending(m => m.IsPrimary)
                .ToListAsync();
        }

        public async Task<IEnumerable<Media>> GetAllByRoomIdAsync(Guid roomId)
        {
            return await _context.Media
                .Where(m => m.RoomId == roomId && !m.IsDeleted)
                .OrderByDescending(m => m.IsPrimary)
                .ToListAsync();
        }

        public async Task<Media?> GetByIdAsync(Guid id)
        {
            return await _context.Media
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<Media> AddAsync(Media media)
        {
            _context.Media.Add(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task UpdateAsync(Media media)
        {
            _context.Media.Update(media);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var media = await _context.Media.FindAsync(id);
            if (media != null)
            {
                media.IsDeleted = true;
                media.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Media.AnyAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<Media?> GetPrimaryByBoardingHouseIdAsync(Guid boardingHouseId, Guid? excludedId = null)
        {
            var query = _context.Media
                .Where(m => m.BoardingHouseId == boardingHouseId && m.IsPrimary && !m.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(m => m.Id != excludedId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Media?> GetPrimaryByRoomIdAsync(Guid roomId, Guid? excludedId = null)
        {
            var query = _context.Media
                .Where(m => m.RoomId == roomId && m.IsPrimary && !m.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(m => m.Id != excludedId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
