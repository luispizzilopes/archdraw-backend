using FluentValidation;

namespace Archdraw.Application.Modules.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(120).WithMessage("O nome deve ter no máximo 120 caracteres.");
        RuleFor(x => x.Request.Email).NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.").MaximumLength(256);
        RuleFor(x => x.Request.Password).NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
    }
}
