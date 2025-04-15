using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(r => r.Id)
            .IsRequired();
        builder.HasKey(r => r.Id);
        builder.HasOne(r => r.SenderUser)
            .WithMany()
            .HasForeignKey(r => r.SenderUserId);
        builder.HasOne(r => r.ReceiverUser)
            .WithMany()
            .HasForeignKey(r => r.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r=> r.RequestedStartDate)
            .IsRequired();
        builder.Property(r => r.Description)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(r => r.PhoneNumber)
            .IsRequired();
        builder.Property(r => r.Address)
            .IsRequired();
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        builder.Property(e => e.UpdatedAt)
            .IsRequired();
        builder.Property(r => r.Status)
            .IsRequired();
        builder.HasMany(r => r.Replies)
            .WithOne(rp => rp.Request)
            .HasForeignKey(rp => rp.RequestId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}