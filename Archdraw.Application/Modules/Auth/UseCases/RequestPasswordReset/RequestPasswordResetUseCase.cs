using System.Security.Cryptography;
using ErrorOr;
using Archdraw.Application.Common.Emails;
using Archdraw.Application.Interfaces.UseCases;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Domain.Modules.Users.Entities;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.UseCases.RequestPasswordReset;

public class RequestPasswordResetUseCase(
    UserManager<AppUser> userManager, IEmailService emailService, IPasswordResetCodeStore codeStore) : IUseCase
{
    public async Task<ErrorOr<Success>> ExecuteAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is { Active: true })
        {
            var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
            await codeStore.StoreAsync(email, code, cancellationToken);
            try
            {
                await emailService.SendAsync(email, EmailTemplates.Subjects.PasswordReset, EmailTemplates.PasswordReset(code), cancellationToken);
            }
            catch
            {
                await codeStore.RemoveAsync(email, cancellationToken);
            }
        }
        return Result.Success;
    }
}
