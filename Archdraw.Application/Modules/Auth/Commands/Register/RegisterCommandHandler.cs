using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.Register;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<LoginResponse>>
{
    private readonly RegisterUseCase _registerUseCase;

    public RegisterCommandHandler(RegisterUseCase registerUseCase)
    {
        _registerUseCase = registerUseCase;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        => await _registerUseCase.ExecuteAsync(command.Request, command.Metadata, cancellationToken);
}
