namespace Archdraw.Application.Modules.Documents.Responses;

public sealed record DocumentResponse(
    string Scope, Guid OwnerId, Guid ProjectId, string Markdown, int Revision, DateTime? UpdatedAtUtc);
