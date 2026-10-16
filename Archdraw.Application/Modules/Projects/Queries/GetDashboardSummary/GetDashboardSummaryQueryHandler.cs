using ErrorOr;
using MediatR;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Application.Modules.Projects.UseCases.GetDashboardSummary;

namespace Archdraw.Application.Modules.Projects.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, ErrorOr<DashboardSummaryResponse>>
{
    private readonly GetDashboardSummaryUseCase _getDashboardSummaryUseCase;

    public GetDashboardSummaryQueryHandler(GetDashboardSummaryUseCase getDashboardSummaryUseCase)
    {
        _getDashboardSummaryUseCase = getDashboardSummaryUseCase;
    }

    public async Task<ErrorOr<DashboardSummaryResponse>> Handle(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
        => await _getDashboardSummaryUseCase.ExecuteAsync(query.UserId, cancellationToken);
}
