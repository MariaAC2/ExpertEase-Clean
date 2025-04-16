using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.Id)
            .IsRequired();
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Balance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(a => a.Currency)
            .HasMaxLength(5)
            .IsRequired();
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.CreatedAt)
            .IsRequired();
        builder.Property(a => a.UpdatedAt)
            .IsRequired();
    }
}