using SpotBoardingHouse.Domain.Enums;

namespace SpotBoardingHouse.Domain.Entities
{
    public class BoardingHouse : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public required string Name { get; set; }
        public string? Description { get; set; }
        public Gender AllowedGender { get; set; }
        public required string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? FacebookLink { get; set; }
    }
}
