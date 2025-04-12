using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class SpecialistConfiguration: IEntityTypeConfiguration<Specialist>
{
    public void Configure(EntityTypeBuilder<Specialist> builder)
    {
        builder.Property(e=> e.UserId)
            .IsRequired();
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(e => e.Address)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(e => e.YearsExperience)
            .IsRequired();
        builder.Property(e => e.Description)
            .HasMaxLength(255);
        builder.Ignore(e => e.CreatedAt);
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.Id);
    }
}