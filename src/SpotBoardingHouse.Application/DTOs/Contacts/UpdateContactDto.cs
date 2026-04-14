using System.ComponentModel.DataAnnotations;
using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Application.DTOs.Contacts
{
    public class UpdateContactDto
    {
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public Network? Network { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
