using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Components.Mappers;
using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Components.UseCases.UpdateComponent;

public class UpdateComponentUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<ComponentResponse>> ExecuteAsync(
        Guid userId, Guid componentId, UpdateComponentRequest request, CancellationToken cancellationToken = default)
    {
        var component = await unitOfWork.ComponentRepository.GetByIdAsync(componentId, userId, cancellationToken);
        if (component is null) return AppErrors.ComponentNotFound;

        component.Update(request.TypeId, request.Properties.ToDomain());
        await unitOfWork.CommitAsync(cancellationToken);

        var usage = await unitOfWork.ComponentRepository.CountUsagesAsync(componentId, cancellationToken);
        return component.ToResponse(usage);
    }
}
