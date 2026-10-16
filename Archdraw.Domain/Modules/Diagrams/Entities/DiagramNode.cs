using Archdraw.BuildingBlocks.Common.Class.Base;
using Archdraw.Domain.Catalogs;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;

namespace Archdraw.Domain.Modules.Diagrams.Entities;

public class DiagramNode : Entity<Guid>
{
    public Guid DiagramId { get; private set; }
    public string TypeId { get; private set; } = string.Empty;
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }
    public int ZIndex { get; private set; }
    public Guid? ComponentId { get; private set; }
    public Guid? LinkedDiagramId { get; private set; }
    public NodeProperties Properties { get; private set; } = NodeProperties.Empty(string.Empty);

    private DiagramNode() { }

    internal static DiagramNode Create(Guid diagramId, NodeData data)
    {
        var node = new DiagramNode { Id = data.Id, DiagramId = diagramId };
        node.Apply(data);
        return node;
    }

    internal void Apply(NodeData data)
    {
        if (data.Id == Guid.Empty) throw new DomainException("Todo nó precisa de um identificador.");
        if (!ArchitecturalCatalog.IsComponentType(data.TypeId))
            throw new DomainException($"Tipo de componente desconhecido: {data.TypeId}.");
        if (!double.IsFinite(data.X) || !double.IsFinite(data.Y) || !double.IsFinite(data.Width) || !double.IsFinite(data.Height))
            throw new DomainException("Posição e dimensões do nó devem ser números finitos.");
        if (data.Width <= 0 || data.Height <= 0)
            throw new DomainException("As dimensões do nó devem ser positivas.");

        TypeId = data.TypeId;
        X = data.X; Y = data.Y; Width = data.Width; Height = data.Height;
        ZIndex = data.ZIndex;
        ComponentId = data.ComponentId;
        LinkedDiagramId = data.LinkedDiagramId;
        Properties = data.Properties;
    }

    internal void ClearLinkedDiagram() => LinkedDiagramId = null;

    public void Detach() => ComponentId = null;
}

public sealed record NodeData(
    Guid Id, string TypeId, double X, double Y, double Width, double Height, int ZIndex,
    Guid? ComponentId, Guid? LinkedDiagramId, NodeProperties Properties);
