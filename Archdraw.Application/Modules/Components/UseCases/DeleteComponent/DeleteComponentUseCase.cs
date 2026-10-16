using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Application.Modules.Components.UseCases.DeleteComponent;

public class DeleteComponentUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(Guid userId, Guid componentId, bool detach, CancellationToken cancellationToken = default)
    {
        var component = await unitOfWork.ComponentRepository.GetByIdAsync(componentId, userId, cancellationToken);
        if (component is null) return AppErrors.ComponentNotFound;

        var usage = await unitOfWork.ComponentRepository.CountUsagesAsync(componentId, cancellationToken);
        if (usage > 0 && !detach)
            return Error.Conflict("Component.InUse", $"O componente é usado em {usage} nó(s). Desvincule-o ou confirme a remoção.",
                new Dictionary<string, object> { ["usageCount"] = usage });

        if (usage > 0) await unitOfWork.ComponentRepository.DetachFromNodesAsync(componentId, cancellationToken);
        await unitOfWork.DocumentRepository.DeleteByOwnerAsync(DocumentScope.Component, componentId, cancellationToken);
        unitOfWork.ComponentRepository.Delete(component);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success;
    }
}
