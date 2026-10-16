using FluentValidation;

namespace Archdraw.Application.Modules.Diagrams.Commands.UpdateDiagram;

public class UpdateDiagramCommandValidator : AbstractValidator<UpdateDiagramCommand>
{
    public UpdateDiagramCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("O nome do diagrama é obrigatório.")
            .MaximumLength(120).WithMessage("O nome do diagrama deve ter no máximo 120 caracteres.");
        RuleFor(x => x.Request.Level).NotEmpty().WithMessage("O nível do diagrama é obrigatório.");
    }
}
