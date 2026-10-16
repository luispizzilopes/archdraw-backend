using FluentValidation;

namespace Archdraw.Application.Modules.Projects.Commands.ImportProject;

public class ImportProjectCommandValidator : AbstractValidator<ImportProjectCommand>
{
    public ImportProjectCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Arquivo de importação inválido.");
        RuleFor(x => x.Request.Project).NotNull().WithMessage("Projeto ausente no arquivo.");
        RuleFor(x => x.Request.Project.Name).NotEmpty().WithMessage("O nome do projeto é obrigatório.")
            .When(x => x.Request.Project is not null);
    }
}
