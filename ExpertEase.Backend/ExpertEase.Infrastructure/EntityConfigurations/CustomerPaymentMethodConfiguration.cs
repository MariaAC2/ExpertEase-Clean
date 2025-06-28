using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class CustomerPaymentMethodConfiguration : IEntityTypeConfiguration<CustomerPaymentMethod>
{
    public void Configure(EntityTypeBuilder<CustomerPaymentMethod> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties configuration
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CustomerId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.StripeCustomerId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.StripePaymentMethodId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.CardLast4)
            .IsRequired()
            .HasMaxLength(4)
            .IsFixedLength();

        builder.Property(e => e.CardBrand)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CardholderName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()"); // PostgreSQL syntax

        // Relationships
        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.StripeCustomerId)
            .HasDatabaseName("IX_CustomerPaymentMethods_StripeCustomerId");

        // Check constraints - PostgreSQL syntax
        builder.HasCheckConstraint("CK_CustomerPaymentMethods_CardLast4_Length", 
            "LENGTH(\"CardLast4\") = 4");

        builder.HasCheckConstraint("CK_CustomerPaymentMethods_CardBrand_Values", 
            "\"CardBrand\" IN ('VISA', 'MASTERCARD', 'AMEX', 'DISCOVER', 'DINERS', 'JCB', 'UNIONPAY', 'UNKNOWN')");
    }
}