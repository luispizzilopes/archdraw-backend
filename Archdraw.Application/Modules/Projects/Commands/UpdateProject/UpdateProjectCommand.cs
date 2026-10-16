using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.UpdateProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(Guid UserId, Guid ProjectId, UpdateProjectRequest Request) : IRequest<ErrorOr<ProjectResponse>>;
