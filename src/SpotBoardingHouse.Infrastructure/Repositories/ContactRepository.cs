using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Domain.Entities;
using SpotBoardingHouse.Infrastructure.Data;

namespace SpotBoardingHouse.Infrastructure.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contact>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            return await _context.Contacts
                .Where(c => c.BoardingHouseId == boardingHouseId && !c.IsDeleted)
                .OrderByDescending(c => c.IsPrimary)
                .ThenBy(c => c.PhoneNumber)
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(Guid id)
        {
            return await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Contact> AddAsync(Contact contact)
        {
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task UpdateAsync(Contact contact)
        {
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                contact.IsDeleted = true;
                contact.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Contacts.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> PhoneExistsAsync(Guid boardingHouseId, string phoneNumber, Guid? excludedId = null)
        {
            var query = _context.Contacts
                .Where(c => c.BoardingHouseId == boardingHouseId && c.PhoneNumber == phoneNumber && !c.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(c => c.Id != excludedId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Contact?> GetPrimaryByBoardingHouseId(Guid boardingHouseId, Guid? excludedId = null)
        {
            var query = _context.Contacts
                .Where(c => c.BoardingHouseId == boardingHouseId && c.IsPrimary && !c.IsDeleted);

            if (excludedId.HasValue)
            {
                query = query.Where(c => c.Id != excludedId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
