using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpotBoardingHouse.Domain.Entities;

namespace SpotBoardingHouse.Infrastructure.Data.Configurations
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.ToTable("Contacts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Network)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.IsPrimary)
                .IsRequired();

            builder.HasOne(c => c.BoardingHouse)
                .WithMany()
                .HasForeignKey(c => c.BoardingHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // One active primary contact per boarding house
            builder.HasIndex(c => c.BoardingHouseId)
                .IsUnique()
                .HasFilter("[IsPrimary] = 1 AND [IsDeleted] = 0");
        }
    }
}
