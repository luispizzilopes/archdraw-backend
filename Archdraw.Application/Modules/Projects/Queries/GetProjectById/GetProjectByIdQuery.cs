using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.GetProjectById;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid UserId, Guid ProjectId) : IRequest<ErrorOr<ProjectResponse>>;
