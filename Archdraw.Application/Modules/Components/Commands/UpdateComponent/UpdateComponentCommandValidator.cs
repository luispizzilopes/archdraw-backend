using FluentValidation;

namespace Archdraw.Application.Modules.Components.Commands.UpdateComponent;

public class UpdateComponentCommandValidator : AbstractValidator<UpdateComponentCommand>
{
    public UpdateComponentCommandValidator()
    {
        RuleFor(x => x.Request.TypeId).NotEmpty().WithMessage("O tipo do componente é obrigatório.");
        RuleFor(x => x.Request.Properties).NotNull().WithMessage("As propriedades do componente são obrigatórias.");
        RuleFor(x => x.Request.Properties.Name).NotEmpty().WithMessage("O nome do componente é obrigatório.")
            .MaximumLength(120).WithMessage("O nome do componente deve ter no máximo 120 caracteres.")
            .When(x => x.Request.Properties is not null);
    }
}
