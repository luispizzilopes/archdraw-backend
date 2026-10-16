using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Application.Modules.Auth.Services;

public static class IdentityErrorMapper
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["DuplicateEmail"] = "Este e-mail já está em uso.",
        ["DuplicateUserName"] = "Este e-mail já está em uso.",
        ["InvalidEmail"] = "E-mail inválido.",
        ["PasswordTooShort"] = "A senha deve ter pelo menos 8 caracteres.",
        ["PasswordRequiresDigit"] = "A senha deve conter ao menos um número.",
        ["PasswordRequiresLower"] = "A senha deve conter ao menos uma letra minúscula.",
        ["PasswordRequiresUpper"] = "A senha deve conter ao menos uma letra maiúscula.",
        ["PasswordRequiresNonAlphanumeric"] = "A senha deve conter ao menos um caractere especial.",
        ["PasswordRequiresUniqueChars"] = "A senha deve ter mais caracteres distintos.",
        ["PasswordMismatch"] = "Senha atual incorreta.",
    };

    public static List<Error> ToErrors(IdentityResult result)
        => result.Errors
            .Select(e => Error.Validation($"Identity.{e.Code}", Messages.GetValueOrDefault(e.Code, e.Description)))
            .ToList();
}
