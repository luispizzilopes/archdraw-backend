using Archdraw.Domain.Modules.Components.Aggregates;
using Archdraw.Domain.Modules.Projects.Aggregates;
using Archdraw.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class ArchitecturalComponentConfiguration : IEntityTypeConfiguration<ArchitecturalComponent>
{
    public void Configure(EntityTypeBuilder<ArchitecturalComponent> builder)
    {
        builder.ToTable("ArchitecturalComponents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.TypeId).HasMaxLength(40).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(ArchitecturalComponent.NameMaxLength).IsRequired();
        builder.Property(c => c.Identifier).HasMaxLength(160).IsRequired();
        builder.Property(c => c.PatternId).HasMaxLength(40);
        builder.Property(c => c.Description).HasMaxLength(ArchitecturalComponent.TextMaxLength).IsRequired();
        builder.Property(c => c.Technology).HasMaxLength(ArchitecturalComponent.TextMaxLength).IsRequired();
        builder.Property(c => c.Responsibility).HasMaxLength(ArchitecturalComponent.TextMaxLength).IsRequired();
        builder.Property(c => c.Notes).HasMaxLength(ArchitecturalComponent.TextMaxLength).IsRequired();
        builder.Property(c => c.Links).HasJsonbConversion();
        builder.Property(c => c.Metadata).HasJsonbConversion();
        builder.Property<uint>("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.HasOne<Project>().WithMany().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => new { c.ProjectId, c.Name });
    }
}
