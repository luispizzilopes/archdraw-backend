using FluentValidation;

namespace Archdraw.Application.Modules.Documents.Commands.SaveDocument;

public class SaveDocumentCommandValidator : AbstractValidator<SaveDocumentCommand>
{
    public SaveDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Markdown).NotNull().WithMessage("O conteúdo é obrigatório.")
            .Must(m => m is null || m.Length <= 200_000).WithMessage("O documento deve ter no máximo 200.000 caracteres.");
        RuleFor(x => x.Request.BaseRevision).GreaterThanOrEqualTo(0).WithMessage("Revisão base inválida.");
    }
}
