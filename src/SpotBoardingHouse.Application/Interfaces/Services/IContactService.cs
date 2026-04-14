using SpotBoardingHouse.Application.DTOs.Contacts;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IContactService
    {
        Task<IEnumerable<ContactDto>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId);
        Task<ContactDto?> GetByIdAsync(Guid id);
        Task<ContactDto> CreateAsync(CreateContactDto createContactDto);
        Task<ContactDto?> UpdateAsync(Guid id, UpdateContactDto updateContactDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
