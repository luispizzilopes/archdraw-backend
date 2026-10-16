using Archdraw.Application.Modules.Auth.Models;

namespace Archdraw.Presentation.Extensions;

public static class AuthSessionHttpExtensions
{
    public const string RefreshTokenCookieName = "archdraw.refresh";
    private const string CookiePath = "/api/v1/authentication";

    public static SessionMetadata GetSessionMetadata(this HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ip = context.Connection.RemoteIpAddress?.ToString();
        return new SessionMetadata(BuildDeviceName(userAgent), Truncate(userAgent, 512), Truncate(ip, 64));
    }

    public static void SetRefreshTokenCookie(this HttpResponse response, string refreshToken, DateTime expiresAtUtc, IWebHostEnvironment environment)
        => response.Cookies.Append(RefreshTokenCookieName, refreshToken, Options(environment, new DateTimeOffset(expiresAtUtc)));

    public static void DeleteRefreshTokenCookie(this HttpResponse response, IWebHostEnvironment environment)
        => response.Cookies.Delete(RefreshTokenCookieName, Options(environment, null));

    private static CookieOptions Options(IWebHostEnvironment environment, DateTimeOffset? expires)
    {
        var local = environment.IsEnvironment("Localhost") || environment.IsDevelopment();
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !local,
            SameSite = local ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expires,
            Path = CookiePath,
            IsEssential = true
        };
    }

    private static string BuildDeviceName(string userAgent)
    {
        var browser = userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase) ? "Edge" :
            userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase) ? "Firefox" :
            userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) ? "Chrome" :
            userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase) ? "Safari" : "Navegador";
        var system = userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ? "iPhone" :
            userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ? "Android" :
            userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase) ? "Windows" :
            userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) ? "macOS" :
            userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase) ? "Linux" : "dispositivo desconhecido";
        return $"{browser} em {system}";
    }

    private static string? Truncate(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? null : value[..Math.Min(value.Length, max)];
}
