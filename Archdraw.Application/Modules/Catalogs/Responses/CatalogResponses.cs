using Archdraw.Domain.Catalogs;

namespace Archdraw.Application.Modules.Catalogs.Responses;

public sealed record CatalogsResponse(
    IReadOnlyList<CatalogItem> Styles, IReadOnlyList<CatalogItem> Patterns,
    IReadOnlyList<CatalogItem> ComponentTypes, IReadOnlyList<CatalogItem> ConnectionKinds);
