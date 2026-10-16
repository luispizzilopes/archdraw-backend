using ErrorOr;
using MediatR;
using Archdraw.Application.Modules.Projects.Responses;

namespace Archdraw.Application.Modules.Projects.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery(Guid UserId) : IRequest<ErrorOr<DashboardSummaryResponse>>;
