using SpotBoardingHouse.Application.DTOs.Contacts;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Application.Services
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IBoardingHouseRepository _boardingHouseRepository;

        public ContactService(IContactRepository contactRepository, IBoardingHouseRepository boardingHouseRepository)
        {
            _contactRepository = contactRepository;
            _boardingHouseRepository = boardingHouseRepository;
        }

        public async Task<IEnumerable<ContactDto>> GetAllByBoardingHouseIdAsync(Guid boardingHouseId)
        {
            var contacts = await _contactRepository.GetAllByBoardingHouseIdAsync(boardingHouseId);
            return contacts.Select(MapToDto);
        }

        public async Task<ContactDto?> GetByIdAsync(Guid id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            return contact == null ? null : MapToDto(contact);
        }

        public async Task<ContactDto> CreateAsync(CreateContactDto createContactDto)
        {
            if (!await _boardingHouseRepository.ExistsAsync(createContactDto.BoardingHouseId))
            {
                throw new InvalidOperationException("Boarding house not found.");
            }

            var phone = createContactDto.PhoneNumber.Trim();

            if (await _contactRepository.PhoneExistsAsync(createContactDto.BoardingHouseId, phone))
            {
                throw new InvalidOperationException("Phone number already exists for boarding house.");
            }

            if (createContactDto.IsPrimary)
            {
                var currentPrimary = await _contactRepository.GetPrimaryByBoardingHouseId(createContactDto.BoardingHouseId);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    await _contactRepository.UpdateAsync(currentPrimary);
                }
            }

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                BoardingHouseId = createContactDto.BoardingHouseId,
                PhoneNumber = phone,
                Network = createContactDto.Network,
                IsPrimary = createContactDto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var created = await _contactRepository.AddAsync(contact);

            return MapToDto(created);
        }

        public async Task<ContactDto?> UpdateAsync(Guid id, UpdateContactDto updateContactDto)
        {
            var existing = await _contactRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            var newPhone = updateContactDto.PhoneNumber?.Trim() ?? existing.PhoneNumber;
            if (await _contactRepository.PhoneExistsAsync(existing.BoardingHouseId, newPhone, id))
            {
                throw new InvalidOperationException("Phone number already exists for this boarding house.");
            }

            var willBePrimary = updateContactDto.IsPrimary ?? existing.IsPrimary;
            if (willBePrimary)
            {
                var currentPrimary = await _contactRepository.GetPrimaryByBoardingHouseId(existing.BoardingHouseId, id);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                    currentPrimary.UpdatedAt = DateTime.UtcNow;
                    await _contactRepository.UpdateAsync(currentPrimary);
                }
            }

            if (updateContactDto.PhoneNumber != null)
            {
                existing.PhoneNumber = newPhone;
            }

            if (updateContactDto.Network.HasValue)
            {
                existing.Network = updateContactDto.Network.Value;
            }

            if (updateContactDto.IsPrimary.HasValue)
            {
                existing.IsPrimary = updateContactDto.IsPrimary.Value;
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _contactRepository.UpdateAsync(existing);

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _contactRepository.ExistsAsync(id))
            {
                return false;
            }

            await _contactRepository.DeleteAsync(id);

            return true;
        }

        private static ContactDto MapToDto(Contact contact)
        {
            return new ContactDto
            {
                Id = contact.Id,
                BoardingHouseId = contact.BoardingHouseId,
                PhoneNumber = contact.PhoneNumber,
                Network = contact.Network.ToString(),
                IsPrimary = contact.IsPrimary,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };
        }
    }
}
