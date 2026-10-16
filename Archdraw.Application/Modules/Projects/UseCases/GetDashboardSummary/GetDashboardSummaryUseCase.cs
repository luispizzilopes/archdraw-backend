using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Projects.UseCases.GetDashboardSummary;

public class GetDashboardSummaryUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<DashboardSummaryResponse>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var s = await unitOfWork.ProjectRepository.GetOwnerSummaryAsync(userId, cancellationToken);
        return new DashboardSummaryResponse(s.Projects, s.Diagrams, s.Components, s.Documents);
    }
}
