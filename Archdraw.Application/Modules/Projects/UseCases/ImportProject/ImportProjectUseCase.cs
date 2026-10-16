using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Domain.Catalogs;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Components.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Aggregates;
using Archdraw.Domain.Modules.Diagrams.Entities;
using Archdraw.Domain.Modules.Diagrams.ValueObjects;
using Archdraw.Domain.Modules.Documents.Aggregates;
using Archdraw.Domain.Modules.Documents.Enums;
using Archdraw.Domain.Modules.Projects.Aggregates;

namespace Archdraw.Application.Modules.Projects.UseCases.ImportProject;

public class ImportProjectUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public const string SupportedFormat = "archdraw.project";
    public const int MaxDiagrams = 200;

    public async Task<ErrorOr<ImportProjectResponse>> ExecuteAsync(
        Guid userId, ImportProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Format != SupportedFormat)
            return Error.Validation("Import.Format", "Arquivo não reconhecido (formato archdraw.project esperado).");
        if (request.Version != 1)
            return Error.Validation("Import.Version", $"Versão de arquivo não suportada: {request.Version}.");

        var legacyDiagrams = request.Diagrams ?? [];
        if (legacyDiagrams.Count > MaxDiagrams)
            return Error.Validation("Import.TooManyDiagrams", $"O arquivo excede o limite de {MaxDiagrams} diagramas.");

        var copies = new Dictionary<string, Guid>();
        Guid Map(string kind, string legacyId)
        {
            var key = $"{kind}:{legacyId}";
            if (!request.AsCopy) return DeterministicGuid.Create(userId, key);
            if (!copies.TryGetValue(key, out var id)) copies[key] = id = Guid.NewGuid();
            return id;
        }

        var projectId = Map("project", request.Project.Id);
        if (!request.AsCopy && await unitOfWork.ProjectRepository.ExistsAsync(projectId, cancellationToken))
            return new ImportProjectResponse(projectId, true, 0, 0, 0, 0, 0, []);

        var warnings = new List<string>();
        var legacy = request.Project;

        var styles = (legacy.SecondaryStyleIds ?? []).Where(ArchitecturalCatalog.IsStyle).ToList();
        var primary = ArchitecturalCatalog.IsStyle(legacy.PrimaryStyleId) ? legacy.PrimaryStyleId : null;
        if (legacy.PrimaryStyleId is not null && primary is null) warnings.Add($"Estilo principal desconhecido ignorado: {legacy.PrimaryStyleId}.");

        var project = Project.Create(projectId, userId, legacy.Name, legacy.Description, primary, styles.Take(Project.MaxSecondaryStyles));
        await unitOfWork.ProjectRepository.CreateAsync(project, cancellationToken);

        var componentCount = 0;
        var componentIds = new Dictionary<string, Guid>();
        foreach (var rc in legacy.ReusableComponents ?? [])
        {
            var props = ToProperties(rc.Properties, rc.TypeId, warnings, out _);
            var component = ArchitecturalComponent.Create(Map("component", rc.Id), projectId, rc.TypeId, props);
            componentIds[rc.Id] = component.Id;
            await unitOfWork.ComponentRepository.CreateAsync(component, cancellationToken);
            componentCount++;
        }

        var diagramIds = legacyDiagrams.ToDictionary(d => d.Id, d => Map("diagram", d.Id));
        int nodeTotal = 0, edgeTotal = 0;

        foreach (var d in legacyDiagrams)
        {
            var diagramId = diagramIds[d.Id];
            var level = ArchitecturalCatalog.DiagramLevels.Contains(d.Level ?? "free") ? d.Level ?? "free" : "free";
            if (level != (d.Level ?? "free")) warnings.Add($"Nível inválido no diagrama \"{d.Name}\"; usado \"free\".");

            var diagram = Diagram.Create(
                diagramId, projectId, d.Name, d.Description, level,
                (d.StyleIds ?? []).Where(ArchitecturalCatalog.IsStyle), (d.PatternIds ?? []).Where(ArchitecturalCatalog.IsPattern));

            var nodeIds = new Dictionary<string, Guid>();
            var nodes = new List<NodeData>();
            foreach (var n in d.Nodes ?? [])
            {
                var nodeId = nodeIds[n.Id] = Map("node", $"{d.Id}/{n.Id}");
                Guid? linked = null;
                if (!string.IsNullOrEmpty(n.Properties.LinkedDiagramId))
                {
                    if (diagramIds.TryGetValue(n.Properties.LinkedDiagramId, out var target) && target != diagramId) linked = target;
                    else warnings.Add($"Vínculo para diagrama inexistente removido em \"{n.Properties.Name}\".");
                }
                Guid? componentId = n.ComponentId is not null && componentIds.TryGetValue(n.ComponentId, out var cid) ? cid : null;
                nodes.Add(new NodeData(
                    nodeId, n.Type, n.Position.X, n.Position.Y, n.Size.Width, n.Size.Height, n.ZIndex,
                    componentId, linked, ToProperties(n.Properties, n.Type, warnings, out _)));
            }

            var edges = new List<ConnectionData>();
            foreach (var e in d.Edges ?? [])
            {
                if (!nodeIds.TryGetValue(e.Source, out var source) || !nodeIds.TryGetValue(e.Target, out var target))
                {
                    warnings.Add($"Conexão órfã ignorada no diagrama \"{d.Name}\".");
                    continue;
                }
                edges.Add(new ConnectionData(
                    Map("edge", $"{d.Id}/{e.Id}"), source, target, e.SourceHandle, e.TargetHandle, e.Kind,
                    e.Label ?? string.Empty, e.Protocol ?? string.Empty, e.Description ?? string.Empty,
                    e.Sync ?? "sync", e.Direction ?? "forward",
                    (e.Waypoints ?? []).Select(p => new Point(p.X, p.Y)).ToList()));
            }

            diagram.ApplySnapshot(
                nodes, edges,
                d.Viewport is null ? Viewport.Default : new Viewport(d.Viewport.X, d.Viewport.Y, d.Viewport.Zoom),
                d.Settings is null ? DiagramSettings.Default : new DiagramSettings(d.Settings.ShowGrid, d.Settings.SnapToGrid, d.Settings.GridSize, d.Settings.ShowMinimap));

            await unitOfWork.DiagramRepository.CreateAsync(diagram, cancellationToken);
            nodeTotal += nodes.Count;
            edgeTotal += edges.Count;
        }

        var documentCount = 0;
        foreach (var doc in request.Documents ?? [])
        {
            Guid? owner = doc.Scope switch
            {
                "project" when doc.OwnerId == legacy.Id => projectId,
                "diagram" when diagramIds.TryGetValue(doc.OwnerId, out var did) => did,
                _ => null
            };
            if (owner is null) { warnings.Add($"Documento sem dono ignorado ({doc.Scope}:{doc.OwnerId})."); continue; }

            var scope = doc.Scope == "project" ? DocumentScope.Project : DocumentScope.Diagram;
            await unitOfWork.DocumentRepository.CreateAsync(Document.Create(projectId, scope, owner.Value, doc.Content ?? string.Empty), cancellationToken);
            documentCount++;
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return new ImportProjectResponse(projectId, false, legacyDiagrams.Count, nodeTotal, edgeTotal, componentCount, documentCount, warnings);
    }

    private static NodeProperties ToProperties(LegacyProperties p, string typeId, List<string> warnings, out bool patternDropped)
    {
        patternDropped = false;
        var pattern = p.PatternId;
        if (pattern is not null && !ArchitecturalCatalog.IsPattern(pattern))
        {
            warnings.Add($"Padrão desconhecido removido em \"{p.Name}\": {pattern}.");
            pattern = null;
            patternDropped = true;
        }

        return new NodeProperties(
            string.IsNullOrWhiteSpace(p.Name) ? typeId : p.Name,
            p.Identifier ?? string.Empty, p.Description ?? string.Empty, p.Technology ?? string.Empty,
            p.Responsibility ?? string.Empty, p.Notes ?? string.Empty, pattern,
            (p.Links ?? []).Select(l => new ComponentLink(l.Label ?? string.Empty, l.Url ?? string.Empty)).ToList(),
            new Dictionary<string, string>(p.Metadata ?? []));
    }
}
