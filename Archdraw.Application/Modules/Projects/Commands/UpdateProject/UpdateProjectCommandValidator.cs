using FluentValidation;

namespace Archdraw.Application.Modules.Projects.Commands.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("O nome do projeto é obrigatório.")
            .MaximumLength(120).WithMessage("O nome do projeto deve ter no máximo 120 caracteres.");
        RuleFor(x => x.Request.Description).MaximumLength(2000).WithMessage("A descrição deve ter no máximo 2000 caracteres.");
    }
}
