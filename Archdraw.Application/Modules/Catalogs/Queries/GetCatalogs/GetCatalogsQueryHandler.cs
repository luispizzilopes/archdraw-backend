using Archdraw.Application.Modules.Catalogs.Responses;
using Archdraw.Application.Modules.Catalogs.UseCases.GetCatalogs;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Catalogs.Queries.GetCatalogs;

public class GetCatalogsQueryHandler : IRequestHandler<GetCatalogsQuery, ErrorOr<CatalogsResponse>>
{
    private readonly GetCatalogsUseCase _getCatalogsUseCase;

    public GetCatalogsQueryHandler(GetCatalogsUseCase getCatalogsUseCase)
    {
        _getCatalogsUseCase = getCatalogsUseCase;
    }

    public async Task<ErrorOr<CatalogsResponse>> Handle(GetCatalogsQuery query, CancellationToken cancellationToken)
        => await _getCatalogsUseCase.ExecuteAsync(cancellationToken);
}
