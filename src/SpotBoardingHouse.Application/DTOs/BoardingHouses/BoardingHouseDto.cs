namespace SpotBoardingHouse.Application.DTOs.BoardingHouses
{
    public class BoardingHouseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string? OwnerName { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AllowedGender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? FacebookLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
