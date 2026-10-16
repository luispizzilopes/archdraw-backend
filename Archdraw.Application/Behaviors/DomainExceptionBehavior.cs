using ErrorOr;
using Archdraw.Domain.Exceptions;
using MediatR;

namespace Archdraw.Application.Behaviors;

public class DomainExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DomainException ex)
        {
            return (dynamic)Error.Validation("Domain.Invalid", ex.Message);
        }
        catch (DuplicateResourceException)
        {
            return (dynamic)Error.Conflict("Resource.Duplicate", "O recurso já existe.");
        }
        catch (ConcurrencyConflictException)
        {
            return (dynamic)Error.Conflict("Concurrency.Conflict", "O recurso foi alterado por outra operação. Recarregue e tente novamente.");
        }
    }
}
