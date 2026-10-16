using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.UseCases.RequestPasswordReset;
using ErrorOr;
using MediatR;

namespace Archdraw.Application.Modules.Auth.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, ErrorOr<Success>>
{
    private readonly RequestPasswordResetUseCase _requestPasswordResetUseCase;

    public RequestPasswordResetCommandHandler(RequestPasswordResetUseCase requestPasswordResetUseCase)
    {
        _requestPasswordResetUseCase = requestPasswordResetUseCase;
    }

    public async Task<ErrorOr<Success>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
        => await _requestPasswordResetUseCase.ExecuteAsync(command.Request, cancellationToken);
}
