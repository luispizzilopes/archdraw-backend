using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Projects.Mappers;
using Archdraw.Application.Modules.Projects.Responses;
using Archdraw.BuildingBlocks.Pagination;
using Archdraw.Domain.Interfaces.UnitOfWork;

namespace Archdraw.Application.Modules.Projects.UseCases.GetProjects;

public class GetProjectsUseCase(IUnitOfWork unitOfWork) : IUseCase
{
    public async Task<ErrorOr<PaginedResult<ProjectResponse>>> ExecuteAsync(
        Guid userId, PaginationParams pagination, string? search, CancellationToken cancellationToken = default)
    {
        var (items, total) = await unitOfWork.ProjectRepository.GetPagedAsync(userId, pagination, search, cancellationToken);
        return new PaginedResult<ProjectResponse>
        {
            Items = items.Select(i => i.Project.ToResponse(i.DiagramCount)).ToList(),
            TotalCount = total,
            PageNumber = pagination.FinalPage,
            PageSize = pagination.FinalSize
        };
    }
}
