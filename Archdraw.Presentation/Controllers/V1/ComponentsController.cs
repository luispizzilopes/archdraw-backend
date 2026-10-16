using Archdraw.Application.Modules.Components.Commands.DeleteComponent;
using Archdraw.Application.Modules.Components.Commands.UpdateComponent;
using Archdraw.Application.Modules.Components.Queries.GetComponentById;
using Archdraw.Application.Modules.Components.Requests;
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
[Route("api/v{version:apiVersion}/components")]
public class ComponentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{componentId:guid}")]
    public async Task<IActionResult> GetById(Guid componentId)
        => (await mediator.Send(new GetComponentByIdQuery(HttpContext.GetUserId(), componentId))).ToActionResult(this);

    [HttpPut("{componentId:guid}")]
    public async Task<IActionResult> Update(Guid componentId, [FromBody] UpdateComponentRequest request)
        => (await mediator.Send(new UpdateComponentCommand(HttpContext.GetUserId(), componentId, request))).ToActionResult(this);

    [HttpDelete("{componentId:guid}")]
    public async Task<IActionResult> Delete(Guid componentId, [FromQuery] bool detach = false)
        => (await mediator.Send(new DeleteComponentCommand(HttpContext.GetUserId(), componentId, detach))).ToActionResult(this);

    [HttpGet("{componentId:guid}/documentation")]
    public async Task<IActionResult> GetDocumentation(Guid componentId)
        => (await mediator.Send(new GetDocumentQuery(HttpContext.GetUserId(), DocumentScope.Component, componentId))).ToActionResult(this);

    [HttpPut("{componentId:guid}/documentation")]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<IActionResult> SaveDocumentation(Guid componentId, [FromBody] SaveDocumentRequest request)
        => (await mediator.Send(new SaveDocumentCommand(HttpContext.GetUserId(), DocumentScope.Component, componentId, request))).ToActionResult(this);
}
