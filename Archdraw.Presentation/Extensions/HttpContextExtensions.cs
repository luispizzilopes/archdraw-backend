namespace Archdraw.Presentation.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext httpContext)
        => Guid.TryParse(httpContext.User.FindFirst("Id")?.Value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Usuário não identificado no token.");

    public static Guid? GetSessionId(this HttpContext httpContext)
        => Guid.TryParse(httpContext.User.FindFirst("sid")?.Value, out var id) ? id : null;
}
