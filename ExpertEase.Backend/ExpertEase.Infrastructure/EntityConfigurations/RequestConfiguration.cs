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
            .HasForeignKey(r => r.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.ReveiverSpecialist)
            .WithMany()
            .HasForeignKey(r => r.ReceiverSpecialistId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r=> r.RequestDate)
            .IsRequired();
        builder.Property(r => r.Description)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        builder.Property(e => e.UpdatedAt)
            .IsRequired();
        builder.Property(r => r.Status)
            .IsRequired();
    }
}