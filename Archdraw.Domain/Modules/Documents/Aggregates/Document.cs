using Archdraw.BuildingBlocks.Common.Class.AggregateRoot;
using Archdraw.BuildingBlocks.Common.Interfaces;
using Archdraw.Domain.Exceptions;
using Archdraw.Domain.Modules.Documents.Enums;

namespace Archdraw.Domain.Modules.Documents.Aggregates;

public class Document : AggregateRoot<Guid>, IAuditableEntity
{
    public const int MaxLength = 200_000;

    public Guid ProjectId { get; private set; }
    public DocumentScope Scope { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Markdown { get; private set; } = string.Empty;
    public int Revision { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Document() { }

    public static Document Create(Guid projectId, DocumentScope scope, Guid ownerId, string markdown)
    {
        var doc = new Document { Id = Guid.NewGuid(), ProjectId = projectId, Scope = scope, OwnerId = ownerId, Revision = 1 };
        Validate(markdown);
        doc.Markdown = markdown;
        return doc;
    }

    public bool IsAtRevision(int baseRevision) => Revision == baseRevision;

    public void Update(string markdown)
    {
        Validate(markdown);
        Markdown = markdown;
        Revision++;
    }

    private static void Validate(string markdown)
    {
        if (markdown is null) throw new DomainException("O conteúdo do documento é obrigatório.");
        if (markdown.Length > MaxLength) throw new DomainException($"O documento deve ter no máximo {MaxLength} caracteres.");
    }

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
