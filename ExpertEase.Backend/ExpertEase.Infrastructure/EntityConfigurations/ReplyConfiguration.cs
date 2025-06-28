using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class ReplyConfiguration : IEntityTypeConfiguration<Reply>
{
    public void Configure(EntityTypeBuilder<Reply> builder)
    {
        // ✅ Primary Key
        builder.Property(r => r.Id)
            .IsRequired();
        builder.HasKey(r => r.Id);

        // ✅ EXISTING: Request relationship
        builder.HasOne(rp => rp.Request)
            .WithMany(r => r.Replies)
            .HasForeignKey(rp => rp.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ EXISTING: Reply properties
        builder.Property(r => r.StartDate)
            .IsRequired();

        builder.Property(r => r.EndDate)
            .IsRequired();

        builder.Property(r => r.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)"); // ✅ ADDED: Specify decimal precision

        builder.Property(r => r.Status)
            .IsRequired();

        // ✅ NEW: Base entity properties
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();
    }
}