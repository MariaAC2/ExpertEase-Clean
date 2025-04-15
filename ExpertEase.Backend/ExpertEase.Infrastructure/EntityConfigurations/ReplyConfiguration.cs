using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class ReplyConfiguration : IEntityTypeConfiguration<Reply>
{
    public void Configure(EntityTypeBuilder<Reply> builder)
    {
        builder.Property(r => r.Id)
            .IsRequired();
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RequestId)
            .IsRequired();
        builder.HasOne(r => r.Request)
            .WithMany()
            .HasForeignKey(r => r.RequestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.StartDate)
            .IsRequired();
        builder.Property(r => r.EndDate)
            .IsRequired();
        builder.Property(r => r.Price)
            .IsRequired();
        builder.Property(r => r.Status)
            .IsRequired();
    }
}