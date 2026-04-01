using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entity;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class BoardingHouseRepository : IBoardingHouseRepository
    {
        private readonly ApplicationDbContext _context;

        public BoardingHouseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BoardingHouse>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.BoardingHouses
                .Where(bh => bh.UserId == userId && !bh.IsDeleted)
                .OrderBy(bh => bh.Name)
                .ToListAsync();
        }

        public async Task<BoardingHouse?> GetByIdAsync(Guid id)
        {
            return await _context.BoardingHouses
                .FirstOrDefaultAsync(bh => bh.Id == id && !bh.IsDeleted);
        }

        public async Task<BoardingHouse> AddAsync(BoardingHouse boardingHouse)
        {
            _context.BoardingHouses.Add(boardingHouse);
            await _context.SaveChangesAsync();
            return boardingHouse;
        }

        public async Task UpdateAsync(BoardingHouse boardingHouse)
        {
            _context.BoardingHouses.Update(boardingHouse);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var boardingHouse = await _context.BoardingHouses.FindAsync(id);
            if (boardingHouse != null)
            {
                boardingHouse.IsDeleted = true;
                boardingHouse.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return _context.BoardingHouses
                .AnyAsync(bh => bh.Id == id && !bh.IsDeleted);
        }

        public async Task<bool> NameExistsForUserAsync(string name, Guid userId, Guid? excludedId = null)
        {
            var query = _context.BoardingHouses
                .Where(bh => bh.UserId == userId && bh.Name == name && !bh.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(bh => bh.Id != excludedId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
