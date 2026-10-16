using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.ImportProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.ImportProject;

public record ImportProjectCommand(Guid UserId, ImportProjectRequest Request) : IRequest<ErrorOr<ImportProjectResponse>>;
