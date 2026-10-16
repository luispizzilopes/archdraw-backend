using ErrorOr;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Application.Modules.Auth.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.Register;

public class RegisterUseCase(UserManager<AppUser> userManager, AuthSessionService authSessionService) : IUseCase
{
    public async Task<ErrorOr<LoginResponse>> ExecuteAsync(
        RegisterRequest request, SessionMetadata metadata, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var user = new AppUser { Name = request.Name.Trim(), Email = email, UserName = email };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return IdentityErrorMapper.ToErrors(result);

        return await authSessionService.CreateAsync(user, metadata, cancellationToken);
    }
}
