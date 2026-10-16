using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Domain.Modules.Projects.Aggregates;
using Archdraw.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class DiagramConfiguration : IEntityTypeConfiguration<Diagram>
{
    public void Configure(EntityTypeBuilder<Diagram> builder)
    {
        builder.ToTable("Diagrams", t =>
        {
            t.HasCheckConstraint("CK_Diagrams_Revision", "\"Revision\" > 0");
            t.HasCheckConstraint("CK_Diagrams_Level", "\"Level\" IN ('free','context','container','component')");
        });
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property(d => d.Name).HasMaxLength(Diagram.NameMaxLength).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(Diagram.DescriptionMaxLength).IsRequired();
        builder.Property(d => d.Level).HasMaxLength(20).IsRequired();
        builder.Property(d => d.StyleIds).HasColumnType("text[]");
        builder.Property(d => d.PatternIds).HasColumnType("text[]");
        builder.Property(d => d.Viewport).HasJsonbConversion();
        builder.Property(d => d.Settings).HasJsonbConversion();
        builder.Property(d => d.Revision).IsConcurrencyToken();

        builder.HasOne<Project>().WithMany().HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(d => new { d.ProjectId, d.UpdatedAtUtc });

        builder.HasMany(d => d.Nodes).WithOne().HasForeignKey(n => n.DiagramId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(d => d.Connections).WithOne().HasForeignKey(c => c.DiagramId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Nodes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(d => d.Connections).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
