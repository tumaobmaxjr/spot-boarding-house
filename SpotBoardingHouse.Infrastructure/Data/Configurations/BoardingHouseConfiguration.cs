using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entity;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    public class BoardingHouseConfiguration : IEntityTypeConfiguration<BoardingHouse>
    {
        public void Configure(EntityTypeBuilder<BoardingHouse> builder)
        {
            builder.ToTable("BoardingHouses");

            builder.HasKey(bh => bh.Id);

            builder.Property(bh => bh.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(bh => bh.Description)
                .HasMaxLength(500);

            builder.Property(bh => bh.AllowedGender)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(bh => bh.Address)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(bh => bh.Latitude)
                .IsRequired();

            builder.Property(bh => bh.Longitude)
                .IsRequired();

            builder.Property(bh => bh.FacebookLink)
                .HasMaxLength(255);

            builder.HasIndex(bh => new { bh.UserId, bh.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(bh => bh.User)
                .WithMany()
                .HasForeignKey(bh => bh.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
