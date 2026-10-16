using FluentValidation;

namespace Archdraw.Application.Modules.Diagrams.Commands.CreateDiagram;

public class CreateDiagramCommandValidator : AbstractValidator<CreateDiagramCommand>
{
    public CreateDiagramCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("O nome do diagrama é obrigatório.")
            .MaximumLength(120).WithMessage("O nome do diagrama deve ter no máximo 120 caracteres.");
        RuleFor(x => x.Request.Level).NotEmpty().WithMessage("O nível do diagrama é obrigatório.");
        When(x => x.Request.Snapshot is not null, () =>
        {
            RuleFor(x => x.Request.Snapshot!.Nodes).NotNull().WithMessage("Nós ausentes.")
                .Must(n => n is null || n.Count <= 2000).WithMessage("Um diagrama pode ter no máximo 2000 nós.");
            RuleFor(x => x.Request.Snapshot!.Edges).NotNull().WithMessage("Conexões ausentes.")
                .Must(e => e is null || e.Count <= 4000).WithMessage("Um diagrama pode ter no máximo 4000 conexões.");
            RuleFor(x => x.Request.Snapshot!.Viewport).NotNull().WithMessage("Viewport ausente.");
            RuleFor(x => x.Request.Snapshot!.Settings).NotNull().WithMessage("Configurações ausentes.");
            RuleForEach(x => x.Request.Snapshot!.Nodes).ChildRules(n =>
            {
                n.RuleFor(i => i.Position).NotNull().WithMessage("Posição do nó ausente.");
                n.RuleFor(i => i.Size).NotNull().WithMessage("Dimensões do nó ausentes.");
                n.RuleFor(i => i.Properties).NotNull().WithMessage("Propriedades do nó ausentes.");
                n.RuleFor(i => i.Type).NotEmpty().WithMessage("Tipo do nó ausente.");
            }).When(x => x.Request.Snapshot!.Nodes is not null);
        });
    }
}
