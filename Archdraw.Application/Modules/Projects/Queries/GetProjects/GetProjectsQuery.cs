using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.GetProjects;
using Archdraw.BuildingBlocks.Pagination;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Queries.GetProjects;

public record GetProjectsQuery(Guid UserId, PaginationParams Pagination, string? Search) : IRequest<ErrorOr<PaginedResult<ProjectResponse>>>;
