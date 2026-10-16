using FluentValidation;

namespace Archdraw.Application.Modules.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().WithMessage("O e-mail é obrigatório.");
        RuleFor(x => x.Request.Password).NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
