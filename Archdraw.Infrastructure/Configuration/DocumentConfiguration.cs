using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Projects.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Archdraw.Infrastructure.Configuration;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property(d => d.Scope).HasConversion<short>();
        builder.Property(d => d.Markdown).IsRequired();
        builder.Property(d => d.Revision).IsConcurrencyToken();

        builder.HasOne<Project>().WithMany().HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(d => new { d.Scope, d.OwnerId }).IsUnique();
        builder.HasIndex(d => d.ProjectId);
    }
}
