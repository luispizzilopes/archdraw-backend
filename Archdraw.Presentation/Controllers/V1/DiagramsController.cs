using Archdraw.Application.Modules.Diagrams.Commands.DeleteDiagram;
using Archdraw.Application.Modules.Diagrams.Commands.DuplicateDiagram;
using Archdraw.Application.Modules.Diagrams.Commands.SaveDiagramSnapshot;
using Archdraw.Application.Modules.Diagrams.Commands.UpdateDiagram;
using Archdraw.Application.Modules.Diagrams.Queries.GetDiagramById;
using Archdraw.Application.Modules.Diagrams.Queries.GetDiagramParents;
using Archdraw.Application.Modules.Diagrams.Queries.GetRecentDiagrams;
using Archdraw.Application.Modules.Diagrams.Queries.GetDiagramSnapshot;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Documents.Commands.SaveDocument;
using Archdraw.Application.Modules.Documents.Queries.GetDocument;
using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Domain.Modules.Documents.Enums;
using Archdraw.Presentation.Extensions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archdraw.Presentation.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/diagrams")]
public class DiagramsController(IMediator mediator) : ControllerBase
{
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 6)
        => (await mediator.Send(new GetRecentDiagramsQuery(HttpContext.GetUserId(), limit))).ToActionResult(this);

    [HttpGet("{diagramId:guid}/parents")]
    public async Task<IActionResult> GetParents(Guid diagramId)
        => (await mediator.Send(new GetDiagramParentsQuery(HttpContext.GetUserId(), diagramId))).ToActionResult(this);

    [HttpGet("{diagramId:guid}")]
    public async Task<IActionResult> GetById(Guid diagramId)
        => (await mediator.Send(new GetDiagramByIdQuery(HttpContext.GetUserId(), diagramId))).ToActionResult(this);

    [HttpPut("{diagramId:guid}")]
    public async Task<IActionResult> Update(Guid diagramId, [FromBody] UpdateDiagramRequest request)
        => (await mediator.Send(new UpdateDiagramCommand(HttpContext.GetUserId(), diagramId, request))).ToActionResult(this);

    [HttpDelete("{diagramId:guid}")]
    public async Task<IActionResult> Delete(Guid diagramId)
        => (await mediator.Send(new DeleteDiagramCommand(HttpContext.GetUserId(), diagramId))).ToActionResult(this);

    [HttpPost("{diagramId:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid diagramId, [FromBody] DuplicateDiagramRequest? request)
        => (await mediator.Send(new DuplicateDiagramCommand(HttpContext.GetUserId(), diagramId, request ?? new DuplicateDiagramRequest(null)))).ToCreatedResult(this);

    [HttpGet("{diagramId:guid}/snapshot")]
    public async Task<IActionResult> GetSnapshot(Guid diagramId)
        => (await mediator.Send(new GetDiagramSnapshotQuery(HttpContext.GetUserId(), diagramId))).ToActionResult(this);

    [HttpPut("{diagramId:guid}/snapshot")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> SaveSnapshot(Guid diagramId, [FromBody] SaveSnapshotRequest request)
        => (await mediator.Send(new SaveDiagramSnapshotCommand(HttpContext.GetUserId(), diagramId, request))).ToActionResult(this);

    [HttpGet("{diagramId:guid}/documentation")]
    public async Task<IActionResult> GetDocumentation(Guid diagramId)
        => (await mediator.Send(new GetDocumentQuery(HttpContext.GetUserId(), DocumentScope.Diagram, diagramId))).ToActionResult(this);

    [HttpPut("{diagramId:guid}/documentation")]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<IActionResult> SaveDocumentation(Guid diagramId, [FromBody] SaveDocumentRequest request)
        => (await mediator.Send(new SaveDocumentCommand(HttpContext.GetUserId(), DocumentScope.Diagram, diagramId, request))).ToActionResult(this);
}
