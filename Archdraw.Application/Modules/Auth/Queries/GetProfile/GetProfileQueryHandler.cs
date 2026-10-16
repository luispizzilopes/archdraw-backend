using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.GetProfile;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ErrorOr<GetProfileResponse>>
{
    private readonly GetProfileUseCase _getProfileUseCase;

    public GetProfileQueryHandler(GetProfileUseCase getProfileUseCase)
    {
        _getProfileUseCase = getProfileUseCase;
    }

    public async Task<ErrorOr<GetProfileResponse>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
        => await _getProfileUseCase.ExecuteAsync(query.UserId, cancellationToken);
}
