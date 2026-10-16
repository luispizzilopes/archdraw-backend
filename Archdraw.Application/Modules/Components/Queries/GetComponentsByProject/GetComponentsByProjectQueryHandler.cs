using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.GetComponentsByProject;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Queries.GetComponentsByProject;

public class GetComponentsByProjectQueryHandler : IRequestHandler<GetComponentsByProjectQuery, ErrorOr<IReadOnlyList<ComponentResponse>>>
{
    private readonly GetComponentsByProjectUseCase _getComponentsByProjectUseCase;

    public GetComponentsByProjectQueryHandler(GetComponentsByProjectUseCase getComponentsByProjectUseCase)
    {
        _getComponentsByProjectUseCase = getComponentsByProjectUseCase;
    }

    public async Task<ErrorOr<IReadOnlyList<ComponentResponse>>> Handle(GetComponentsByProjectQuery query, CancellationToken cancellationToken)
        => await _getComponentsByProjectUseCase.ExecuteAsync(query.UserId, query.ProjectId, cancellationToken);
}
