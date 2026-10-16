using ErrorOr;

namespace Archdraw.Application.Common;

public static class AppErrors
{
    public static Error ProjectNotFound => Error.NotFound("Project.NotFound", "Projeto não encontrado.");
    public static Error DiagramNotFound => Error.NotFound("Diagram.NotFound", "Diagrama não encontrado.");
    public static Error ComponentNotFound => Error.NotFound("Component.NotFound", "Componente não encontrado.");
    public static Error UserNotFound => Error.NotFound("AppUser.NotFound", "Usuário não encontrado.");

    public static Error RevisionConflict(string code, long currentRevision) => Error.Conflict(
        code,
        "O recurso foi alterado por outra sessão. Recarregue ou escolha qual versão manter.",
        new Dictionary<string, object> { ["currentRevision"] = currentRevision });
}
