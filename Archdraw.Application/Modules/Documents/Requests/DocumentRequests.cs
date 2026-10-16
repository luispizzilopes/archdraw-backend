namespace Archdraw.Application.Modules.Documents.Requests;

public sealed record SaveDocumentRequest(string Markdown, int BaseRevision);
