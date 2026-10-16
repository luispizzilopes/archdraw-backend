using Archdraw.BuildingBlocks.Common.Class.AggregateRoot;
using Archdraw.BuildingBlocks.Common.Interfaces;
using Archdraw.Domain.Catalogs;
using Archdraw.Domain.Exceptions;

namespace Archdraw.Domain.Modules.Projects.Aggregates;

public class Project : AggregateRoot<Guid>, IAuditableEntity
{
    public const int NameMaxLength = 120;
    public const int DescriptionMaxLength = 2000;
    public const int MaxSecondaryStyles = 3;

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? PrimaryStyleId { get; private set; }
    public List<string> SecondaryStyleIds { get; private set; } = [];
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Project() { }

    public static Project Create(
        Guid id, Guid ownerId, string name, string? description,
        string? primaryStyleId, IEnumerable<string>? secondaryStyleIds)
    {
        if (ownerId == Guid.Empty) throw new DomainException("O proprietário do projeto é obrigatório.");
        var project = new Project { Id = id == Guid.Empty ? Guid.NewGuid() : id, OwnerId = ownerId };
        project.Apply(name, description, primaryStyleId, secondaryStyleIds);
        return project;
    }

    public void Update(string name, string? description, string? primaryStyleId, IEnumerable<string>? secondaryStyleIds)
        => Apply(name, description, primaryStyleId, secondaryStyleIds);

    public bool IsOwnedBy(Guid userId) => OwnerId == userId;

    private void Apply(string name, string? description, string? primaryStyleId, IEnumerable<string>? secondaryStyleIds)
    {
        name = (name ?? string.Empty).Trim();
        if (name.Length == 0) throw new DomainException("O nome do projeto é obrigatório.");
        if (name.Length > NameMaxLength) throw new DomainException($"O nome do projeto deve ter no máximo {NameMaxLength} caracteres.");
        description = (description ?? string.Empty).Trim();
        if (description.Length > DescriptionMaxLength) throw new DomainException($"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        if (primaryStyleId is not null && !ArchitecturalCatalog.IsStyle(primaryStyleId))
            throw new DomainException($"Estilo arquitetural desconhecido: {primaryStyleId}.");

        var secondary = (secondaryStyleIds ?? []).Distinct().Where(s => s != primaryStyleId).ToList();
        if (secondary.Any(s => !ArchitecturalCatalog.IsStyle(s)))
            throw new DomainException("Estilo arquitetural secundário desconhecido.");
        if (secondary.Count > MaxSecondaryStyles)
            throw new DomainException($"No máximo {MaxSecondaryStyles} estilos secundários.");

        Name = name;
        Description = description;
        PrimaryStyleId = primaryStyleId;
        SecondaryStyleIds = secondary;
    }

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
