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
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Amount actually transferred to specialist");

        builder.Property(p => p.RefundedAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Amount refunded to client");

        builder.Property(p => p.PlatformRevenue)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Platform's actual revenue from this payment");

        builder.Property(p => p.FeeCollected)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Whether platform fee has been secured");

        // ✅ Status and required fields
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>() // Store enum as string for readability
            .HasMaxLength(50); // ✅ INCREASED: More room for new enum values like "Escrowed"

        builder.Property(p => p.StripeAccountId)
            .IsRequired()
            .HasMaxLength(255);

        // ✅ Currency configuration
        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("RON")
            .IsFixedLength()
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
            .HasComment("When client completed payment");

        builder.Property(p => p.EscrowReleasedAt)
            .HasComment("When money was transferred to specialist");

        builder.Property(p => p.CancelledAt)
            .HasComment("When payment was cancelled");

        builder.Property(p => p.RefundedAt)
            .HasComment("When payment was refunded");

        // ✅ JSON field for protection fee details
        builder.Property(p => p.ProtectionFeeDetailsJson)
            .HasColumnType("nvarchar(max)") // ✅ CHANGED: Use SQL Server compatible type instead of PostgreSQL jsonb
            .HasComment("JSON serialized protection fee calculation details");

        // ✅ Optional service task reference
        builder.Property(p => p.ServiceTaskId)
            .HasComment("Associated service task ID");

        // ✅ UPDATED: Base entity properties (CreatedAt, UpdatedAt, etc.)
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasComment("When payment record was created");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasComment("When payment record was last updated");

        // ✅ FIXED: Change from WithOne() to WithMany()
        builder.HasOne(p => p.Reply)
            .WithMany() // This indicates Reply can have many Payments
            .HasForeignKey(p => p.ReplyId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Indexes for performance
        builder.HasIndex(p => p.StripePaymentIntentId)
            .IsUnique()
            .HasDatabaseName("IX_Payment_StripePaymentIntentId")
            .HasFilter("[StripePaymentIntentId] IS NOT NULL"); // ✅ ADDED: Filter for partial unique index

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payment_Status");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Payment_CreatedAt");

        builder.HasIndex(p => p.PaidAt)
            .HasDatabaseName("IX_Payment_PaidAt")
            .HasFilter("[PaidAt] IS NOT NULL"); // ✅ ADDED: Filter since PaidAt is nullable

        builder.HasIndex(p => new { p.Status, p.CreatedAt })
            .HasDatabaseName("IX_Payment_Status_CreatedAt");

        // ✅ ADDED: Index for escrow queries
        builder.HasIndex(p => new { p.Status, p.TransferredAmount, p.RefundedAmount })
            .HasDatabaseName("IX_Payment_Escrow_Status")
            .HasFilter("[Status] IN ('Escrowed', 'Completed')");

        // ✅ Check constraints for data integrity
        builder.HasCheckConstraint("CK_Payment_ServiceAmount_NonNegative", 
            "[ServiceAmount] >= 0");

        builder.HasCheckConstraint("CK_Payment_ProtectionFee_NonNegative", 
            "[ProtectionFee] >= 0");

        builder.HasCheckConstraint("CK_Payment_TotalAmount_Valid", 
            "[TotalAmount] = [ServiceAmount] + [ProtectionFee]");

        builder.HasCheckConstraint("CK_Payment_TransferredAmount_Valid", 
            "[TransferredAmount] >= 0 AND [TransferredAmount] <= [ServiceAmount]");

        builder.HasCheckConstraint("CK_Payment_RefundedAmount_Valid", 
            "[RefundedAmount] >= 0 AND [RefundedAmount] <= [TotalAmount]");

        // ✅ ADDED: Additional business logic constraints
        builder.HasCheckConstraint("CK_Payment_PlatformRevenue_Valid",
            "[PlatformRevenue] >= 0 AND [PlatformRevenue] <= [ProtectionFee]");

        // ✅ ADDED: Ensure escrow release timestamp is after payment timestamp
        builder.HasCheckConstraint("CK_Payment_EscrowRelease_After_Payment",
            "[EscrowReleasedAt] IS NULL OR [PaidAt] IS NULL OR [EscrowReleasedAt] >= [PaidAt]");

        // ✅ Table configuration
        builder.ToTable("Payments", schema: "dbo");
        
        // ✅ Add table comment
        builder.HasComment("Payment records with escrow support for secure service transactions");
    }
}