using Archdraw.BuildingBlocks.Common.Class.AggregateRoot;
using Archdraw.BuildingBlocks.Common.Interfaces;
using Archdraw.Domain.Catalogs;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;

namespace Archdraw.Domain.Modules.Components.Aggregates;

public class ArchitecturalComponent : AggregateRoot<Guid>, IAuditableEntity
{
    public const int NameMaxLength = 120;
    public const int TextMaxLength = 4000;

    public Guid ProjectId { get; private set; }
    public string TypeId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Identifier { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Technology { get; private set; } = string.Empty;
    public string Responsibility { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public string? PatternId { get; private set; }
    public List<ComponentLink> Links { get; private set; } = [];
    public Dictionary<string, string> Metadata { get; private set; } = [];
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ArchitecturalComponent() { }

    public static ArchitecturalComponent Create(Guid id, Guid projectId, string typeId, NodeProperties properties)
    {
        if (projectId == Guid.Empty) throw new DomainException("O projeto do componente é obrigatório.");
        var component = new ArchitecturalComponent { Id = id == Guid.Empty ? Guid.NewGuid() : id, ProjectId = projectId };
        component.Apply(typeId, properties);
        return component;
    }

    public void Update(string typeId, NodeProperties properties) => Apply(typeId, properties);

    public NodeProperties ToProperties() =>
        new(Name, Identifier, Description, Technology, Responsibility, Notes, PatternId,
            Links.ToList(), new Dictionary<string, string>(Metadata));

    private void Apply(string typeId, NodeProperties p)
    {
        if (!ArchitecturalCatalog.IsComponentType(typeId)) throw new DomainException($"Tipo de componente desconhecido: {typeId}.");
        var name = (p.Name ?? string.Empty).Trim();
        if (name.Length == 0) throw new DomainException("O nome do componente é obrigatório.");
        if (name.Length > NameMaxLength) throw new DomainException($"O nome do componente deve ter no máximo {NameMaxLength} caracteres.");
        if (p.PatternId is not null && !ArchitecturalCatalog.IsPattern(p.PatternId)) throw new DomainException("Padrão arquitetural desconhecido.");
        foreach (var text in new[] { p.Description, p.Technology, p.Responsibility, p.Notes })
            if ((text ?? string.Empty).Length > TextMaxLength) throw new DomainException($"Os textos do componente devem ter no máximo {TextMaxLength} caracteres.");

        TypeId = typeId;
        Name = name;
        Identifier = (p.Identifier ?? string.Empty).Trim();
        Description = p.Description ?? string.Empty;
        Technology = p.Technology ?? string.Empty;
        Responsibility = p.Responsibility ?? string.Empty;
        Notes = p.Notes ?? string.Empty;
        PatternId = p.PatternId;
        Links = (p.Links ?? []).ToList();
        Metadata = new Dictionary<string, string>(p.Metadata ?? []);
    }

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
