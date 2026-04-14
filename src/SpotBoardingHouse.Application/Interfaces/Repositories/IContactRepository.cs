using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Interfaces.Repositories
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId);
        Task<Contact?> GetByIdAsync(Guid id);
        Task<Contact> AddAsync(Contact contact);
        Task UpdateAsync(Contact contact);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> PhoneExistsAsync(Guid boardingHouseId, string phoneNumber, Guid? excludedId = null);
        Task<Contact?> GetPrimaryByBoardingHouseId(Guid boardingHouseId, Guid? excludedId = null);
    }
}
