using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .Include(r => r.BoardingHouse)
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.Rooms
                .Include(r => r.BoardingHouse)
                .Where(r => r.BoardingHouse.UserId == userId && !r.IsDeleted)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Room>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            return await _context.Rooms
                .Include(r => r.BoardingHouse)
                .Where(r => r.BoardingHouseId == boardingHouseId && !r.IsDeleted)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms
                .Include(r => r.BoardingHouse)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Room> AddAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.IsDeleted = true;
                room.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Rooms.AnyAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<bool> RoomNumberExistsAsync(string roomNumber, Guid boardingHouseId, Guid? excludeId = null)
        {
            var query = _context.Rooms
                .Where(r => r.BoardingHouseId == boardingHouseId && r.RoomNumber == roomNumber && !r.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
