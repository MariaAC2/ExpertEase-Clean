using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class ServiceTaskConfiguration: IEntityTypeConfiguration<ServiceTask>
{
    public void Configure(EntityTypeBuilder<ServiceTask> builder)
    {
        builder.HasKey(st => st.Id);
        builder.Property(st => st.UserId)
            .IsRequired();
        builder.Property(st => st.SpecialistId)
            .IsRequired();
        builder.Property(st => st.StartDate)
            .IsRequired();
        builder.Property(st => st.EndDate)
            .IsRequired();
        builder.Property(st => st.Description)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(st => st.Address)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(st => st.Status)
            .IsRequired();
        builder.Property(st => st.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.HasOne(st=> st.Payment)
            .WithOne()
            .HasForeignKey<ServiceTask>(st => st.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}