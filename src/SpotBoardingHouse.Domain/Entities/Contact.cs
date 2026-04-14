using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Domain.Entities
{
    public class Contact : BaseEntity
    {
        public Guid BoardingHouseId { get; set; }
        public BoardingHouse BoardingHouse { get; set; } = null!;

        public required string PhoneNumber { get; set; }
        public Network Network { get; set; }
        public bool IsPrimary { get; set; }
    }
}
