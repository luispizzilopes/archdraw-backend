using FluentValidation;

namespace Archdraw.Application.Modules.Auth.Commands.ConfirmPasswordReset;

public class ConfirmPasswordResetCommandValidator : AbstractValidator<ConfirmPasswordResetCommand>
{
    public ConfirmPasswordResetCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Request.Code).NotEmpty().WithMessage("Informe o código.")
            .Matches("^[0-9]{6}$").WithMessage("O código deve ter 6 dígitos.");
        RuleFor(x => x.Request.NewPassword).NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}
