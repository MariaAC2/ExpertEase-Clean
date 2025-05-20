using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class ContactInfoConfiguration: IEntityTypeConfiguration<ContactInfo>
{
    public void Configure(EntityTypeBuilder<ContactInfo> builder)
    {
        builder.Property(ci=> ci.UserId)
            .IsRequired();
        builder.HasKey(e => e.UserId);
        builder.Property(ci => ci.PhoneNumber).IsRequired();
        builder.Property(ci => ci.Address).IsRequired();
        // builder.HasOne(ci => ci.User)
        //     .WithOne(u => u.ContactInfo)
        //     .HasForeignKey<ContactInfo>(ci => ci.userId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}