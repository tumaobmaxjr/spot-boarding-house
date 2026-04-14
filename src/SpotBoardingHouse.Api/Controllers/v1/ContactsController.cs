using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Contacts;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly IBoardingHouseService _boardingHouseService;

        public ContactsController(IContactService contactService, IBoardingHouseService boardingHouseService)
        {
            _contactService = contactService;
            _boardingHouseService = boardingHouseService;
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out userId);
        }

        private async Task<(bool Exists, bool Authorized)> TryAuthorizeBoardingHouseAsync(Guid boardingHouseId)
        {
            var boardingHouse = await _boardingHouseService.GetByIdAsync(boardingHouseId);
            if (boardingHouse == null)
            {
                return (false, false);
            }

            if (User.IsInRole("Admin"))
            {
                return (true, true);
            }

            if (!TryGetCurrentUserId(out var userId))
            {
                return (true, false);
            }

            return (true, boardingHouse.UserId == userId);
        }

        private async Task<(ContactDto? Contact, bool Exists, bool Authorized)> TryAuthorizeContactAsync(Guid contactId)
        {
            var contact = await _contactService.GetByIdAsync(contactId);
            if (contact == null)
            {
                return (null, false, false);
            }

            var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(contact.BoardingHouseId);
            if (!exists) return (null, false, false);
            if (!authorized) return (contact, true, false);

            return (contact, true, true);
        }

        [HttpGet("boarding-house/{boardingHouseId:guid}")]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetByBoardingHouse(Guid boardingHouseId)
        {
            var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(boardingHouseId);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();

            var contacts = await _contactService.GetAllByBoardingHouseIdAsync(boardingHouseId);

            return Ok(contacts);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ContactDto>> GetById(Guid id)
        {
            var (contact, exists, authorized) = await TryAuthorizeContactAsync(id);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (contact == null) return NotFound();

            return Ok(contact);
        }

        [HttpPost]
        public async Task<ActionResult<ContactDto>> Create([FromBody] CreateContactDto createContactDto)
        {
            try
            {
                var (exists, authorized) = await TryAuthorizeBoardingHouseAsync(createContactDto.BoardingHouseId);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();

                var created = await _contactService.CreateAsync(createContactDto);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ContactDto>> Update(Guid id, [FromBody] UpdateContactDto updateContactDto)
        {
            try
            {
                var (existing, exists, authorized) = await TryAuthorizeContactAsync(id);

                if (!exists) return NotFound();
                if (!authorized) return Forbid();
                if (existing == null) return NotFound();

                var updated = await _contactService.UpdateAsync(id, updateContactDto);

                return updated == null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (existing, exists, authorized) = await TryAuthorizeContactAsync(id);

            if (!exists) return NotFound();
            if (!authorized) return Forbid();
            if (existing == null) return NotFound();

            var deleted = await _contactService.DeleteAsync(id);

            return deleted ? NoContent() : NotFound();
        }
    }
}
