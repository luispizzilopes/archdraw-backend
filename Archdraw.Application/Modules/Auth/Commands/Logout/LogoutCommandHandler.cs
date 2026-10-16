using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.Logout;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    private readonly LogoutUseCase _logoutUseCase;

    public LogoutCommandHandler(LogoutUseCase logoutUseCase)
    {
        _logoutUseCase = logoutUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(LogoutCommand command, CancellationToken cancellationToken)
        => await _logoutUseCase.ExecuteAsync(command.RefreshToken, cancellationToken);
}
