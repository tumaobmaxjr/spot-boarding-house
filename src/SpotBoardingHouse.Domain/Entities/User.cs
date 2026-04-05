namespace SpotBoardingHouse.Domain.Entities
{
    public class User : BaseEntity
    {
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
