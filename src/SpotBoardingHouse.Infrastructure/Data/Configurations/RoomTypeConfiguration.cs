using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> builder)
        {
            builder.ToTable("RoomTypes", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_RoomType_MinOccupants", "MinOccupants > 0");
                tableBuilder.HasCheckConstraint("CK_RoomType_MaxOccupants", "MaxOccupants >= MinOccupants");
            });

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(rt => rt.Description)
                .HasMaxLength(255);

            builder.Property(rt => rt.MinOccupants)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(rt => rt.MaxOccupants)
                .IsRequired()
                .HasDefaultValue(1);

            builder.HasIndex(rt => rt.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}
