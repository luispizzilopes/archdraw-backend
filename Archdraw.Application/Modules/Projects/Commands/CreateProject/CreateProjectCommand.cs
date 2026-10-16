using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.CreateProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.CreateProject;

public record CreateProjectCommand(Guid UserId, CreateProjectRequest Request) : IRequest<ErrorOr<ProjectResponse>>;
