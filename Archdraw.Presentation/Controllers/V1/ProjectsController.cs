using Archdraw.Application.Modules.Components.Commands.CreateComponent;
using Archdraw.Application.Modules.Components.Queries.GetComponentsByProject;
using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Diagrams.Commands.CreateDiagram;
using Archdraw.Application.Modules.Diagrams.Queries.GetDiagramsByProject;
using Archdraw.Application.Modules.Diagrams.Requests;
using Archdraw.Application.Modules.Documents.Commands.SaveDocument;
using Archdraw.Application.Modules.Documents.Queries.GetDocument;
using Archdraw.Application.Modules.Documents.Requests;
using Archdraw.Application.Modules.Projects.Commands.CreateProject;
using Archdraw.Application.Modules.Projects.Commands.DeleteProject;
using Archdraw.Application.Modules.Projects.Commands.ImportProject;
using Archdraw.Application.Modules.Projects.Commands.UpdateProject;
using Archdraw.Application.Modules.Projects.Queries.GetDashboardSummary;
using Archdraw.Application.Modules.Projects.Queries.GetProjectById;
using Archdraw.Application.Modules.Projects.Queries.GetProjects;
using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.BuildingBlocks.Pagination;
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
[Route("api/v{version:apiVersion}/projects")]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => (await mediator.Send(new GetProjectsQuery(
            HttpContext.GetUserId(), new PaginationParams { PageNumber = pageNumber, PageSize = pageSize }, search))).ToActionResult(this);

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
        => (await mediator.Send(new GetDashboardSummaryQuery(HttpContext.GetUserId()))).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
        => (await mediator.Send(new CreateProjectCommand(HttpContext.GetUserId(), request))).ToCreatedResult(this);

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetById(Guid projectId)
        => (await mediator.Send(new GetProjectByIdQuery(HttpContext.GetUserId(), projectId))).ToActionResult(this);

    [HttpPut("{projectId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, [FromBody] UpdateProjectRequest request)
        => (await mediator.Send(new UpdateProjectCommand(HttpContext.GetUserId(), projectId, request))).ToActionResult(this);

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId)
        => (await mediator.Send(new DeleteProjectCommand(HttpContext.GetUserId(), projectId))).ToActionResult(this);

    [HttpPost("import")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Import([FromBody] ImportProjectRequest request)
        => (await mediator.Send(new ImportProjectCommand(HttpContext.GetUserId(), request))).ToActionResult(this);

    [HttpGet("{projectId:guid}/diagrams")]
    public async Task<IActionResult> GetDiagrams(Guid projectId)
        => (await mediator.Send(new GetDiagramsByProjectQuery(HttpContext.GetUserId(), projectId))).ToActionResult(this);

    [HttpPost("{projectId:guid}/diagrams")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CreateDiagram(Guid projectId, [FromBody] CreateDiagramRequest request)
        => (await mediator.Send(new CreateDiagramCommand(HttpContext.GetUserId(), projectId, request))).ToCreatedResult(this);

    [HttpGet("{projectId:guid}/components")]
    public async Task<IActionResult> GetComponents(Guid projectId)
        => (await mediator.Send(new GetComponentsByProjectQuery(HttpContext.GetUserId(), projectId))).ToActionResult(this);

    [HttpPost("{projectId:guid}/components")]
    public async Task<IActionResult> CreateComponent(Guid projectId, [FromBody] CreateComponentRequest request)
        => (await mediator.Send(new CreateComponentCommand(HttpContext.GetUserId(), projectId, request))).ToCreatedResult(this);

    [HttpGet("{projectId:guid}/documentation")]
    public async Task<IActionResult> GetDocumentation(Guid projectId)
        => (await mediator.Send(new GetDocumentQuery(HttpContext.GetUserId(), DocumentScope.Project, projectId))).ToActionResult(this);

    [HttpPut("{projectId:guid}/documentation")]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<IActionResult> SaveDocumentation(Guid projectId, [FromBody] SaveDocumentRequest request)
        => (await mediator.Send(new SaveDocumentCommand(HttpContext.GetUserId(), DocumentScope.Project, projectId, request))).ToActionResult(this);
}
