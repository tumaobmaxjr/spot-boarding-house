using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    internal class RoomOfferConfiguration : IEntityTypeConfiguration<RoomOffer>
    {
        public void Configure(EntityTypeBuilder<RoomOffer> builder)
        {
            builder.ToTable("RoomOffers", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_RoomOffer_Capacity", "Capacity > 0");
                tableBuilder.HasCheckConstraint("CK_RoomOffer_MinPrice", "MinPrice >= 0");
                tableBuilder.HasCheckConstraint("CK_RoomOffer_MaxPrice", "MaxPrice >= MinPrice");
            });

            builder.HasKey(ro => ro.Id);

            builder.Property(ro => ro.Capacity)
                .IsRequired();

            builder.Property(ro => ro.MinPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(ro => ro.MaxPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(ro => ro.IsAvailable)
                .IsRequired();

            builder.HasIndex(ro => new { ro.RoomId, ro.RoomTypeId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(ro => ro.Room)
                .WithMany()
                .HasForeignKey(ro => ro.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ro => ro.RoomType)
                .WithMany()
                .HasForeignKey(ro => ro.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
