using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.ConfirmPasswordReset;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.ConfirmPasswordReset;

public class ConfirmPasswordResetCommandHandler : IRequestHandler<ConfirmPasswordResetCommand, ErrorOr<Success>>
{
    private readonly ConfirmPasswordResetUseCase _confirmPasswordResetUseCase;

    public ConfirmPasswordResetCommandHandler(ConfirmPasswordResetUseCase confirmPasswordResetUseCase)
    {
        _confirmPasswordResetUseCase = confirmPasswordResetUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(ConfirmPasswordResetCommand command, CancellationToken cancellationToken)
        => await _confirmPasswordResetUseCase.ExecuteAsync(command.Request, cancellationToken);
}
