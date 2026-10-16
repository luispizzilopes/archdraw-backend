using ErrorOr;
using Archdraw.Application.Common;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Components.Mappers;
using Archdraw.Application.Modules.Components.Requests;
using Archdraw.Application.Modules.Components.Responses;
using Archdraw.Application.Modules.Diagrams.Mappers;
using Archdraw.Domain.Interfaces.UnitOfWork;
using Archdraw.Domain.Modules.Components.Aggregates;

namespace Archdraw.Application.Modules.Components.UseCases.CreateComponent;

public class CreateComponentUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<ComponentResponse>> ExecuteAsync(
        Guid userId, Guid projectId, CreateComponentRequest request, CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.ProjectRepository.GetByIdAsync(projectId, userId, cancellationToken);
        if (project is null) return AppErrors.ProjectNotFound;

        var id = request.Id ?? Guid.NewGuid();
        if ((await unitOfWork.ComponentRepository.GetByIdsInProjectAsync(projectId, [id], cancellationToken)).Count > 0)
            return Error.Conflict("Component.AlreadyExists", "Já existe um componente com este identificador.");

        var component = ArchitecturalComponent.Create(id, projectId, request.TypeId, request.Properties.ToDomain());
        await unitOfWork.ComponentRepository.CreateAsync(component, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return component.ToResponse(0);
    }
}
