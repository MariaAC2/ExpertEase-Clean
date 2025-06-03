using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Id)
            .IsRequired();
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Content)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(r => r.Rating)
            .IsRequired();
        builder.Property(r => r.CreatedAt)
            .IsRequired();
        builder.Property(r => r.UpdatedAt)
            .IsRequired();
        builder.HasOne(r => r.SenderUser)
            .WithMany()
            .HasForeignKey(r => r.SenderUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.ReceiverUser)
            .WithMany()
            .HasForeignKey(r => r.ReceiverUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.ServiceTask)
            .WithOne(st => st.Review)
            .HasForeignKey<Review>(r => r.ServiceTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}