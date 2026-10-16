using FluentValidation;

namespace Archdraw.Application.Modules.Diagrams.Commands.SaveDiagramSnapshot;

public class SaveDiagramSnapshotCommandValidator : AbstractValidator<SaveDiagramSnapshotCommand>
{
    public SaveDiagramSnapshotCommandValidator()
    {
        RuleFor(x => x.Request.BaseRevision).GreaterThanOrEqualTo(0).WithMessage("Revisão base inválida.");
        RuleFor(x => x.Request.Nodes).NotNull().WithMessage("Nós ausentes.")
            .Must(n => n is null || n.Count <= 2000).WithMessage("Um diagrama pode ter no máximo 2000 nós.");
        RuleFor(x => x.Request.Edges).NotNull().WithMessage("Conexões ausentes.")
            .Must(e => e is null || e.Count <= 4000).WithMessage("Um diagrama pode ter no máximo 4000 conexões.");
        RuleFor(x => x.Request.Viewport).NotNull().WithMessage("Viewport ausente.");
        RuleFor(x => x.Request.Settings).NotNull().WithMessage("Configurações ausentes.");
        RuleForEach(x => x.Request.Nodes).ChildRules(n =>
        {
            n.RuleFor(i => i.Position).NotNull().WithMessage("Posição do nó ausente.");
            n.RuleFor(i => i.Size).NotNull().WithMessage("Dimensões do nó ausentes.");
            n.RuleFor(i => i.Properties).NotNull().WithMessage("Propriedades do nó ausentes.");
            n.RuleFor(i => i.Type).NotEmpty().WithMessage("Tipo do nó ausente.");
        }).When(x => x.Request.Nodes is not null);
    }
}
