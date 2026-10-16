using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Archdraw.Application.Interfaces.Services;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Archdraw.Infrastructure.Services;

public class TokenJwtService(IConfiguration configuration) : ITokenJwtService
{
    public TokenJwtInformationResponse CreateTokenUser(AppUser user, Guid sessionId)
    {
        var expiration = DateTime.UtcNow.AddMinutes(double.Parse(configuration["TokenConfiguration:AccessTokenExpireMinutes"] ?? "15"));
        var token = new JwtSecurityToken(
            issuer: configuration["TokenConfiguration:Issuer"],
            audience: configuration["TokenConfiguration:Audience"],
            claims: CreateClaims(user, sessionId),
            expires: expiration,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:key"]!)), SecurityAlgorithms.HmacSha256));

        return new TokenJwtInformationResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), DateExpiration = expiration };
    }

    public string CreateRefreshToken() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    private static Claim[] CreateClaims(AppUser user, Guid sessionId) =>
    [
        new("Id", user.Id.ToString()),
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new("sid", sessionId.ToString()),
        new(ClaimTypes.Email, user.Email ?? string.Empty),
        new(ClaimTypes.Name, user.Name)
    ];
}
