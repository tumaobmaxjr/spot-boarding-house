using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.ToTable("Media", tableBuilder =>
            {
                // Exactly one target must be set:
                // (BoardingHouseId set, RoomId null) OR (BoardingHouseId null, RoomId set)
                tableBuilder.HasCheckConstraint(
                    "CK_Media_TargetXor",
                    "(([BoardingHouseId] IS NOT NULL AND [RoomId] IS NULL) OR ([BoardingHouseId] IS NULL AND [RoomId] IS NOT NULL))"
                    );
            });

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Url)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.IsPrimary)
                .IsRequired();

            builder.Property(m => m.DisplayOrder)
                .IsRequired();

            builder.HasOne(m => m.BoardingHouse)
                .WithMany()
                .HasForeignKey(m => m.BoardingHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // One active primary per boarding house
            builder.HasIndex(m => m.BoardingHouseId)
                .IsUnique()
                .HasFilter("[BoardingHouseId] IS NOT NULL AND [IsPrimary] = 1 AND [IsDeleted] = 0");

            builder.HasOne(m => m.Room)
                .WithMany()
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // One active primary per room
            builder.HasIndex(m => m.RoomId)
                .IsUnique()
                .HasFilter("[RoomId] IS NOT NULL AND [IsPrimary] = 1 AND [IsDeleted] = 0");
        }
    }
}
