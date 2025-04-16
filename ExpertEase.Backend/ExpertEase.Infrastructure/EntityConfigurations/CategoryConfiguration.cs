using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpertEase.Infrastructure.EntityConfigurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Id)
            .IsRequired();
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(50);
        builder.HasMany(x=>x.Specialists)
            .WithMany(x=>x.Categories)
            .UsingEntity(j => j.ToTable("SpecialistCategories"));
    }
}