using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class RoomOfferRepository : IRoomOfferRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomOfferRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomOffer>> GetAllByRoomIdAsync(Guid roomId)
        {
            return await _context.RoomOffers
                .Include(ro => ro.RoomType)
                .Where(ro => ro.RoomId == roomId && !ro.IsDeleted)
                .ToListAsync();
        }

        public async Task<RoomOffer?> GetByIdAsync(Guid id)
        {
            return await _context.RoomOffers
                .Include(ro => ro.RoomType)
                .FirstOrDefaultAsync(ro => ro.Id == id && !ro.IsDeleted);
        }

        public async Task<RoomOffer> AddAsync(RoomOffer roomOffer)
        {
            _context.RoomOffers.Add(roomOffer);
            await _context.SaveChangesAsync();
            return roomOffer;
        }

        public async Task UpdateAsync(RoomOffer roomOffer)
        {
            _context.RoomOffers.Update(roomOffer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var roomOffer = await _context.RoomOffers.FindAsync(id);
            if (roomOffer != null)
            {
                roomOffer.IsDeleted = true;
                roomOffer.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.RoomOffers.AnyAsync(ro => ro.Id == id && !ro.IsDeleted);
        }

        public async Task<bool> TypeExistsForRoomAsync(Guid roomId, Guid roomTypeId, Guid? excludedId = null)
        {
            var query = _context.RoomOffers
                .Where(ro => ro.RoomId == roomId && ro.RoomTypeId == roomTypeId && !ro.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(ro => ro.Id != excludedId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
