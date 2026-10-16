using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Application.Modules.Diagrams.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Diagrams.UseCases.GetRecentDiagrams;

public class GetRecentDiagramsUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<IReadOnlyList<DiagramResponse>>> ExecuteAsync(Guid userId, int limit, CancellationToken cancellationToken = default)
        => (await unitOfWork.DiagramRepository.ListRecentAsync(userId, limit, cancellationToken)).Select(s => s.ToResponse()).ToList();
}
