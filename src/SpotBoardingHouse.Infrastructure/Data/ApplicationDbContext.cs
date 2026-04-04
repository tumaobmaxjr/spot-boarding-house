using Microsoft.EntityFrameworkCore;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<BoardingHouse> BoardingHouses => Set<BoardingHouse>();
        public DbSet<RoomType> RoomTypes => Set<RoomType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
