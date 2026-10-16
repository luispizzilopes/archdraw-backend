using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.GetProjects;
using Archdraw.BuildingBlocks.Pagination;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, ErrorOr<PaginedResult<ProjectResponse>>>
{
    private readonly GetProjectsUseCase _getProjectsUseCase;

    public GetProjectsQueryHandler(GetProjectsUseCase getProjectsUseCase)
    {
        _getProjectsUseCase = getProjectsUseCase;
    }

    public async Task<ErrorOr<PaginedResult<ProjectResponse>>> Handle(GetProjectsQuery query, CancellationToken cancellationToken)
        => await _getProjectsUseCase.ExecuteAsync(query.UserId, query.Pagination, query.Search, cancellationToken);
}
