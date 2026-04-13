using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    public class BoardingHouseAmenityConfiguration : IEntityTypeConfiguration<BoardingHouseAmenity>
    {
        public void Configure(EntityTypeBuilder<BoardingHouseAmenity> builder)
        {
            builder.ToTable("BoardingHouseAmenities");

            builder.HasKey(x => new { x.BoardingHouseId, x.AmenityId });

            builder.HasOne(x => x.BoardingHouse)
                .WithMany()
                .HasForeignKey(x => x.BoardingHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Amenity)
                .WithMany(a => a.BoardingHouseAmenities)
                .HasForeignKey(x => x.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
