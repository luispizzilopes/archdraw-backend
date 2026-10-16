using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.RefreshSession;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, ErrorOr<LoginResponse>>
{
    private readonly RefreshSessionUseCase _refreshSessionUseCase;

    public RefreshSessionCommandHandler(RefreshSessionUseCase refreshSessionUseCase)
    {
        _refreshSessionUseCase = refreshSessionUseCase;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
        => await _refreshSessionUseCase.ExecuteAsync(command.RefreshToken, command.Metadata, cancellationToken);
}
