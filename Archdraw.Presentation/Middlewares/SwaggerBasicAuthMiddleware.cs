using System.Security.Cryptography;
using System.Text;

namespace Archdraw.Presentation.Middlewares;

public sealed class SwaggerBasicAuthMiddleware
{
    private const string BasicScheme = "Basic ";
    private readonly RequestDelegate _next;
    private readonly byte[] _user;
    private readonly byte[] _password;

    public SwaggerBasicAuthMiddleware(RequestDelegate next, string username, string password)
    {
        _next = next;
        _user = Hash(username);
        _password = Hash(password);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HasValidCredentials(context.Request.Headers.Authorization))
        {
            await _next(context);
            return;
        }
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\", charset=\"UTF-8\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private bool HasValidCredentials(string? header)
    {
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith(BasicScheme, StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header[BasicScheme.Length..].Trim()));
            var i = credentials.IndexOf(':');
            if (i < 0) return false;
            return CryptographicOperations.FixedTimeEquals(Hash(credentials[..i]), _user) &
                   CryptographicOperations.FixedTimeEquals(Hash(credentials[(i + 1)..]), _password);
        }
        catch (FormatException) { return false; }
    }

    private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
}
