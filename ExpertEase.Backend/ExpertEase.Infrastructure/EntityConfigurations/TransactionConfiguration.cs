using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(e=>e.Id)
            .IsRequired();
        builder.HasKey(e=>e.Id);
        builder.Property(e => e.SenderUserId)
            .IsRequired();
        builder.HasOne(e => e.SenderUser)
            .WithMany()
            .HasForeignKey(e => e.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.ReceiverSpecialistId)
            .IsRequired();
        builder.HasOne(e => e.ReveiverSpecialist)
            .WithMany()
            .HasForeignKey(e => e.ReceiverSpecialistId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(e => e.Status)
            .IsRequired();
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        builder.Property(e => e.UpdatedAt)
            .IsRequired();
    }
}