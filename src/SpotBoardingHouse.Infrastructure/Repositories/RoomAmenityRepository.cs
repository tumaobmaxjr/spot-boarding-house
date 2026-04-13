using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class RoomAmenityRepository : IRoomAmenityRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomAmenityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Amenity>> GetAmenitiesByRoomIdAsync(Guid roomId)
        {
            return await _context.RoomAmenities
                .Where(x => x.RoomId == roomId)
                .Select(x => x.Amenity)
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid roomId, Guid amenityId)
        {
            return await _context.RoomAmenities
                .AnyAsync(x => x.RoomId == roomId && x.AmenityId == amenityId);
        }

        public async Task AddAsync(RoomAmenity roomAmenity)
        {
            _context.RoomAmenities.Add(roomAmenity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid roomId, Guid amenityId)
        {
            var roomAmenity = await _context.RoomAmenities
                .FirstOrDefaultAsync(x => x.RoomId == roomId && x.AmenityId == amenityId);

            if (roomAmenity != null)
            {
                _context.RoomAmenities.Remove(roomAmenity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
