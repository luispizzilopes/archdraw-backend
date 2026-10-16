using System.Text.Json.Serialization;

namespace Archdraw.Application.Modules.Auth.Responses;

public sealed class TokenJwtInformationResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime DateExpiration { get; set; }
}

public sealed record LoginResponse(
    Guid UserId, string? Email, string Name, string Token, DateTime TokenExpiresAtUtc, string? ProfilePhotoUrl)
{
    [JsonIgnore] public string RefreshToken { get; init; } = string.Empty;
    [JsonIgnore] public DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public sealed record GetProfileResponse(
    Guid UserId, string Name, string? Email, string? ProfilePhotoUrl, DateTime CreatedAtUtc);
