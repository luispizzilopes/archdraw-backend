using Archdraw.BuildingBlocks.Common.Interfaces;
using Archdraw.BuildingBlocks.Common.Interfaces.Events;
using Archdraw.Domain.Modules.Components.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Projects.Aggregates;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Archdraw.Infrastructure.Context;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<Diagram> Diagrams { get; set; }
    public virtual DbSet<DiagramNode> DiagramNodes { get; set; }
    public virtual DbSet<DiagramConnection> DiagramConnections { get; set; }
    public virtual DbSet<ArchitecturalComponent> ArchitecturalComponents { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Ignore<List<IDomainEvent>>();
        foreach (var entity in modelBuilder.Model.GetEntityTypes().ToList())
            if (typeof(IHasDomainEvent).IsAssignableFrom(entity.ClrType))
                modelBuilder.Entity(entity.ClrType).Ignore(nameof(IHasDomainEvent.DomainEvents));
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps(DateTime.UtcNow);
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps(DateTime.UtcNow);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void ApplyAuditTimestamps(DateTime now)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.SetCreatedAt(now);
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.SetUpdatedAt(now);
        }
    }
}
