using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_Room_BaseCapacity", "BaseCapacity > 0");
                tableBuilder.HasCheckConstraint("CK_Room_OccupiedSlots", "OccupiedSlots <= BaseCapacity");
            });

            builder.HasKey(r => r.Id);

            builder.Property(r => r.RoomNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.BaseCapacity)
                .IsRequired();

            builder.Property(r => r.OccupiedSlots)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.AdditionalDescription)
                .HasMaxLength(500);

            builder.HasIndex(r => new { r.BoardingHouseId, r.RoomNumber })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(r => r.BoardingHouse)
                .WithMany()
                .HasForeignKey(r => r.BoardingHouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
