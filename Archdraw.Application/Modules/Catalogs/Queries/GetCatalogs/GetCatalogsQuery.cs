using Archdraw.Application.Modules.Catalogs.Responses;
using Archdraw.Application.Modules.Catalogs.UseCases.GetCatalogs;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Catalogs.Queries.GetCatalogs;

public record GetCatalogsQuery() : IRequest<ErrorOr<CatalogsResponse>>;
