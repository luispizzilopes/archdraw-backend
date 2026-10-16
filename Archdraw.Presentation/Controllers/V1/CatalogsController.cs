using Archdraw.Application.Modules.Catalogs.Queries.GetCatalogs;
using Archdraw.Presentation.Extensions;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archdraw.Presentation.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/catalogs")]
public class CatalogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get() => (await mediator.Send(new GetCatalogsQuery())).ToActionResult(this);
}
