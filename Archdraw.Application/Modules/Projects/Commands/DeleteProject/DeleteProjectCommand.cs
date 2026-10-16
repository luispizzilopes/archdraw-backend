using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.DeleteProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid UserId, Guid ProjectId) : IRequest<ErrorOr<Success>>;
