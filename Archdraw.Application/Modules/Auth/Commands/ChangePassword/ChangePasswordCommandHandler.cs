using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.ChangePassword;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ErrorOr<Success>>
{
    private readonly ChangePasswordUseCase _changePasswordUseCase;

    public ChangePasswordCommandHandler(ChangePasswordUseCase changePasswordUseCase)
    {
        _changePasswordUseCase = changePasswordUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        => await _changePasswordUseCase.ExecuteAsync(command.UserId, command.SessionId, command.Request, cancellationToken);
}
