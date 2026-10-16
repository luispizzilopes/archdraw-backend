using FluentValidation;

namespace Archdraw.Application.Modules.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("O nome do projeto é obrigatório.")
            .MaximumLength(120).WithMessage("O nome do projeto deve ter no máximo 120 caracteres.");
        RuleFor(x => x.Request.Description).MaximumLength(2000).WithMessage("A descrição deve ter no máximo 2000 caracteres.");
    }
}
