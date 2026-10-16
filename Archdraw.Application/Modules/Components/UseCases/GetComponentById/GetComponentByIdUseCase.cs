using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Components.Mappers;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Components.UseCases.GetComponentById;

public class GetComponentByIdUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<ComponentResponse>> ExecuteAsync(Guid userId, Guid componentId, CancellationToken cancellationToken = default)
    {
        var component = await unitOfWork.ComponentRepository.GetByIdAsync(componentId, userId, cancellationToken);
        if (component is null) return AppErrors.ComponentNotFound;

        var usage = await unitOfWork.ComponentRepository.CountUsagesAsync(componentId, cancellationToken);
        return component.ToResponse(usage);
    }
}
