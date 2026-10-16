using FluentValidation;

namespace Archdraw.Application.Modules.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(120).WithMessage("O nome deve ter no máximo 120 caracteres.");
    }
}
