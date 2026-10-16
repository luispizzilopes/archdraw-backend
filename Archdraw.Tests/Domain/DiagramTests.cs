using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;

namespace Archdraw.Tests.Domain;

public class DiagramTests
{
    private static NodeData Node(Guid? id = null, string type = "api-rest") =>
        new(id ?? Guid.NewGuid(), type, 0, 0, 200, 80, 1, null, null, NodeProperties.Empty("n"));

    private static ConnectionData Edge(Guid source, Guid target, string kind = "http") =>
        new(Guid.NewGuid(), source, target, null, null, kind, "", "", "", "sync", "forward", []);

    private static Diagram NewDiagram() => Diagram.Create(Guid.NewGuid(), Guid.NewGuid(), "D", null, "free", ["microservices"], []);

    [Fact]
    public void Create_comeca_na_revisao_1_e_valida_nome_nivel_e_catalogos()
    {
        Assert.Equal(1, NewDiagram().Revision);
        Assert.Throws<DomainException>(() => Diagram.Create(Guid.NewGuid(), Guid.NewGuid(), " ", null, "free", [], []));
        Assert.Throws<DomainException>(() => Diagram.Create(Guid.NewGuid(), Guid.NewGuid(), "D", null, "nivel-x", [], []));
        Assert.Throws<DomainException>(() => Diagram.Create(Guid.NewGuid(), Guid.NewGuid(), "D", null, "free", ["estilo-x"], []));
        Assert.Throws<DomainException>(() => Diagram.Create(Guid.NewGuid(), Guid.Empty, "D", null, "free", [], []));
    }

    [Fact]
    public void ApplySnapshot_incrementa_revisao_e_substitui_conteudo()
    {
        var d = NewDiagram();
        var a = Node(); var b = Node();
        d.ApplySnapshot([a, b], [Edge(a.Id, b.Id)], Viewport.Default, DiagramSettings.Default);
        Assert.Equal(2, d.Revision);
        Assert.Equal(2, d.Nodes.Count);

        d.ApplySnapshot([a], [], Viewport.Default, DiagramSettings.Default);
        Assert.Equal(3, d.Revision);
        Assert.Single(d.Nodes);
        Assert.Empty(d.Connections);
    }

    [Fact]
    public void ApplySnapshot_mantem_a_identidade_dos_nos_existentes()
    {
        var d = NewDiagram();
        var a = Node();
        d.ApplySnapshot([a], [], Viewport.Default, DiagramSettings.Default);
        var tracked = d.Nodes.Single();

        d.ApplySnapshot([a with { X = 99 }], [], Viewport.Default, DiagramSettings.Default);
        Assert.Same(tracked, d.Nodes.Single());
        Assert.Equal(99, tracked.X);
    }

    [Fact]
    public void Conexao_para_no_inexistente_viola_invariante()
    {
        var d = NewDiagram();
        var a = Node();
        Assert.Throws<DomainException>(() => d.ApplySnapshot([a], [Edge(a.Id, Guid.NewGuid())], Viewport.Default, DiagramSettings.Default));
        Assert.Equal(1, d.Revision);
    }

    [Fact]
    public void Ids_duplicados_tipos_e_dimensoes_invalidas_sao_rejeitados()
    {
        var d = NewDiagram();
        var a = Node();
        Assert.Throws<DomainException>(() => d.ApplySnapshot([a, a], [], Viewport.Default, DiagramSettings.Default));
        Assert.Throws<DomainException>(() => d.ApplySnapshot([Node(type: "inexistente")], [], Viewport.Default, DiagramSettings.Default));
        Assert.Throws<DomainException>(() => d.ApplySnapshot([a with { Width = 0 }], [], Viewport.Default, DiagramSettings.Default));
        Assert.Throws<DomainException>(() => d.ApplySnapshot([a with { X = double.NaN }], [], Viewport.Default, DiagramSettings.Default));
        var b = Node();
        Assert.Throws<DomainException>(() => d.ApplySnapshot([a, b], [Edge(a.Id, b.Id, "kind-x")], Viewport.Default, DiagramSettings.Default));
        Assert.Throws<DomainException>(() => d.ApplySnapshot([], [], new Viewport(0, 0, 0), DiagramSettings.Default));
    }

    [Fact]
    public void Limite_de_nos_e_aplicado()
    {
        var d = NewDiagram();
        var nodes = Enumerable.Range(0, Diagram.MaxNodes + 1).Select(_ => Node()).ToList();
        Assert.Throws<DomainException>(() => d.ApplySnapshot(nodes, [], Viewport.Default, DiagramSettings.Default));
    }

    [Fact]
    public void Duplicate_gera_novos_ids_remapeia_conexoes_e_preserva_componentes()
    {
        var d = NewDiagram();
        var componentId = Guid.NewGuid();
        var a = Node() with { ComponentId = componentId }; var b = Node();
        d.ApplySnapshot([a, b], [Edge(a.Id, b.Id)], Viewport.Default, DiagramSettings.Default);

        var copy = d.Duplicate("Cópia");

        Assert.NotEqual(d.Id, copy.Id);
        Assert.Equal(1, copy.Revision);
        Assert.Equal(2, copy.Nodes.Count);
        Assert.Empty(copy.Nodes.Select(n => n.Id).Intersect(d.Nodes.Select(n => n.Id)));
        var edge = copy.Connections.Single();
        Assert.Contains(copy.Nodes, n => n.Id == edge.SourceNodeId);
        Assert.Contains(copy.Nodes, n => n.Id == edge.TargetNodeId);
        Assert.Equal(componentId, copy.Nodes.Single(n => n.ComponentId.HasValue).ComponentId);
    }
}
