using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.Login;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly LoginUseCase _loginUseCase;

    public LoginCommandHandler(LoginUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        => await _loginUseCase.ExecuteAsync(command.Request, command.Metadata, cancellationToken);
}
