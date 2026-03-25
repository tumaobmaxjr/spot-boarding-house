using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entity;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
        {
            var query = _context.Users
                .Where(u => u.Email == email && !u.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(u => u.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            return await _context.Users
                .AnyAsync(r => r.Id == roleId && !r.IsDeleted);
        }
    }
}
