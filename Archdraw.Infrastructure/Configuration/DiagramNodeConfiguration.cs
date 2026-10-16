using Archdraw.Domain.Modules.Components.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class DiagramNodeConfiguration : IEntityTypeConfiguration<DiagramNode>
{
    public void Configure(EntityTypeBuilder<DiagramNode> builder)
    {
        builder.ToTable("DiagramNodes", t => t.HasCheckConstraint("CK_DiagramNodes_Size", "\"Width\" > 0 AND \"Height\" > 0"));
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.TypeId).HasMaxLength(40).IsRequired();
        builder.Property(n => n.Properties).HasJsonbConversion();

        builder.HasAlternateKey(n => new { n.DiagramId, n.Id });

        builder.HasOne<ArchitecturalComponent>().WithMany().HasForeignKey(n => n.ComponentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Diagram>().WithMany().HasForeignKey(n => n.LinkedDiagramId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.ComponentId);
        builder.HasIndex(n => n.LinkedDiagramId);
    }
}
