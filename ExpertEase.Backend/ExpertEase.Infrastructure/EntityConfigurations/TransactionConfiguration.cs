using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(100);
        
        builder.Property(e => e.Summary)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.Property(e => e.TransactionType).IsRequired();
        builder.HasOne(e => e.InitiatorUser)
            .WithMany()
            .HasForeignKey(e => e.InitiatorUserId);

        builder.HasOne(e => e.SenderUser)
            .WithMany()
            .HasForeignKey(e => e.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReceiverUser)
            .WithMany()
            .HasForeignKey(e => e.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SenderAccount)
            .WithMany()
            .HasForeignKey(e => e.SenderAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReceiverAccount)
            .WithMany()
            .HasForeignKey(e => e.ReceiverAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

}