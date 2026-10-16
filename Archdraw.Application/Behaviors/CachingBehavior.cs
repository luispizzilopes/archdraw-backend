using Archdraw.Application.Interfaces.Caching;
using Archdraw.Domain.Interfaces.Caching;
using MediatR;

namespace Archdraw.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse>(ICacheService cacheService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable) return await next(cancellationToken);

        var cached = await cacheService.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken);
        if (cached is not null) return cached;

        var response = await next(cancellationToken);
        await cacheService.SetAsync(cacheable.CacheKey, response, cacheable.Expiration, cancellationToken);
        return response;
    }
}
