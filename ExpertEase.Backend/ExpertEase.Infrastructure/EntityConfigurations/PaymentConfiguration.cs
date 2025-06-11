using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class PaymentConfiguration: IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(x => x.Id)
            .IsRequired();
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount)
            .IsRequired();
        builder.Property(p => p.Status)
            .IsRequired();
        builder.Property(p => p.StripeAccountId)
            .IsRequired();
        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("RON");
        builder.HasOne(p => p.ServiceTask)
            .WithOne(st => st.Payment)
            .HasForeignKey<Payment>(p => p.ServiceTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}