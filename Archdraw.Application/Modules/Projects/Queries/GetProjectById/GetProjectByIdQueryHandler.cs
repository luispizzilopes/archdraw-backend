using Archdraw.Application.Modules.Projects.Requests;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.GetProjectById;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ErrorOr<ProjectResponse>>
{
    private readonly GetProjectByIdUseCase _getProjectByIdUseCase;

    public GetProjectByIdQueryHandler(GetProjectByIdUseCase getProjectByIdUseCase)
    {
        _getProjectByIdUseCase = getProjectByIdUseCase;
    }

    public async Task<ErrorOr<ProjectResponse>> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
        => await _getProjectByIdUseCase.ExecuteAsync(query.UserId, query.ProjectId, cancellationToken);
}
