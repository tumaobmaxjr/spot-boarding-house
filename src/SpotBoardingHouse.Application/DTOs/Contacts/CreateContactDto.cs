using System.ComponentModel.DataAnnotations;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.DTOs.Contacts
{
    public class CreateContactDto
    {
        [Required]
        public Guid BoardingHouseId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public Network Network { get; set; }

        public bool IsPrimary { get; set; }
    }
}
