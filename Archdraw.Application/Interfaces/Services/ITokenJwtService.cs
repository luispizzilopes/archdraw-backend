using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Domain.Modules.Users.Entities;

namespace Archdraw.Application.Interfaces.Services;

public interface ITokenJwtService
{
    TokenJwtInformationResponse CreateTokenUser(AppUser user, Guid sessionId);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
