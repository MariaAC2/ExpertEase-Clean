using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class SpecialistProfileConfiguration: IEntityTypeConfiguration<SpecialistProfile>
{
    public void Configure(EntityTypeBuilder<SpecialistProfile> builder)
    {
        builder.Property(e=> e.UserId)
            .IsRequired();
        builder.HasKey(e => e.UserId);
        builder.Property(e => e.YearsExperience)
            .IsRequired();
        builder.Property(e => e.YearsExperienceString)
            .IsRequired();
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        builder.Property(e => e.UpdatedAt)
            .IsRequired();
        builder.Ignore(e => e.Id);
    }
}