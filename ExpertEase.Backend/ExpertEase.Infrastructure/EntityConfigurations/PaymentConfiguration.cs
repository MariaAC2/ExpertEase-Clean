using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // ✅ Primary Key
        builder.Property(x => x.Id)
            .IsRequired();
        builder.HasKey(p => p.Id);
        
        // ✅ CORRECT: Keep your navigation property relationship
        builder.HasOne(p => p.Reply)
            .WithMany(r => r.Payments) // This is fine if Reply has List<Payment> Payments
            .HasForeignKey(p => p.ReplyId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ UPDATED: New amount structure for escrow support
        builder.Property(p => p.ServiceAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Amount that will be transferred to specialist");

        builder.Property(p => p.ProtectionFee)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Platform protection fee");

        builder.Property(p => p.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Total amount charged to client (ServiceAmount + ProtectionFee)");

        // ✅ Financial tracking fields
        builder.Property(p => p.TransferredAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Amount actually transferred to specialist");

        builder.Property(p => p.RefundedAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Amount refunded to client");

        builder.Property(p => p.PlatformRevenue)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Platform's actual revenue from this payment");

        builder.Property(p => p.FeeCollected)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Whether platform fee has been secured");

        // ✅ Status and required fields
        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.StripeAccountId)
            .IsRequired()
            .HasMaxLength(255);

        // ✅ Currency configuration
        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("RON")
            .HasComment("ISO currency code");

        // ✅ Stripe integration fields
        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(255)
            .HasComment("Stripe payment intent ID");

        builder.Property(p => p.StripeChargeId)
            .HasMaxLength(255)
            .HasComment("Stripe charge ID");

        builder.Property(p => p.StripeTransferId)
            .HasMaxLength(255)
            .HasComment("Stripe transfer ID when money sent to specialist");

        builder.Property(p => p.StripeRefundId)
            .HasMaxLength(255)
            .HasComment("Stripe refund ID if payment was refunded");

        // ✅ Timestamp fields
        builder.Property(p => p.PaidAt)
            .IsRequired(false)
            .HasComment("When client completed payment");

        builder.Property(p => p.EscrowReleasedAt)
            .IsRequired(false)
            .HasComment("When money was transferred to specialist");

        builder.Property(p => p.CancelledAt)
            .IsRequired(false)
            .HasComment("When payment was cancelled");

        builder.Property(p => p.RefundedAt)
            .IsRequired(false)
            .HasComment("When payment was refunded");

        // ✅ JSON field for PostgreSQL
        builder.Property(p => p.ProtectionFeeDetailsJson)
            .HasColumnType("jsonb")
            .IsRequired(false)
            .HasComment("JSON serialized protection fee calculation details");

        // ✅ Optional service task reference
        builder.Property(p => p.ServiceTaskId)
            .IsRequired(false)
            .HasComment("Associated service task ID");

        // ✅ Base entity properties
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasComment("When payment record was created");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasComment("When payment record was last updated");

        // ✅ Indexes for performance
        builder.HasIndex(p => p.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("\"StripePaymentIntentId\" IS NOT NULL");
        
        // ✅ Add table comment
        builder.HasComment("Payment records with escrow support for secure service transactions");
    }
}