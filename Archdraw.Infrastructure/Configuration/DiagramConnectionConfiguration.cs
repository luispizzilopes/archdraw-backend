using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class DiagramConnectionConfiguration : IEntityTypeConfiguration<DiagramConnection>
{
    public void Configure(EntityTypeBuilder<DiagramConnection> builder)
    {
        builder.ToTable("DiagramConnections");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.KindId).HasMaxLength(40).IsRequired();
        builder.Property(c => c.Label).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Protocol).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000).IsRequired();
        builder.Property(c => c.Sync).HasMaxLength(10).IsRequired();
        builder.Property(c => c.Direction).HasMaxLength(10).IsRequired();
        builder.Property(c => c.SourceHandle).HasMaxLength(40);
        builder.Property(c => c.TargetHandle).HasMaxLength(40);
        builder.Property(c => c.Waypoints).HasJsonbConversion();

        builder.HasOne<DiagramNode>().WithMany()
            .HasForeignKey(c => new { c.DiagramId, c.SourceNodeId })
            .HasPrincipalKey(n => new { n.DiagramId, n.Id })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<DiagramNode>().WithMany()
            .HasForeignKey(c => new { c.DiagramId, c.TargetNodeId })
            .HasPrincipalKey(n => new { n.DiagramId, n.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.DiagramId, c.SourceNodeId });
        builder.HasIndex(c => new { c.DiagramId, c.TargetNodeId });
    }
}
