using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Catalogs.Responses;
using Archdraw.Domain.Catalogs;

namespace Archdraw.Application.Modules.Catalogs.UseCases.GetCatalogs;

public class GetCatalogsUseCase : IUseCase
{
    public Task<ErrorOr<CatalogsResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<ErrorOr<CatalogsResponse>>(new CatalogsResponse(
            ArchitecturalCatalog.Styles, ArchitecturalCatalog.Patterns,
            ArchitecturalCatalog.ComponentTypes, ArchitecturalCatalog.ConnectionKinds));
}
