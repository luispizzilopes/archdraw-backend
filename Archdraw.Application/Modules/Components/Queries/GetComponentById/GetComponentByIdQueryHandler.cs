using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Components.UseCases.GetComponentById;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Components.Queries.GetComponentById;

public class GetComponentByIdQueryHandler : IRequestHandler<GetComponentByIdQuery, ErrorOr<ComponentResponse>>
{
    private readonly GetComponentByIdUseCase _getComponentByIdUseCase;

    public GetComponentByIdQueryHandler(GetComponentByIdUseCase getComponentByIdUseCase)
    {
        _getComponentByIdUseCase = getComponentByIdUseCase;
    }

    public async Task<ErrorOr<ComponentResponse>> Handle(GetComponentByIdQuery query, CancellationToken cancellationToken)
        => await _getComponentByIdUseCase.ExecuteAsync(query.UserId, query.ComponentId, cancellationToken);
}
