using Archdraw.Application.Modules.Diagrams.Requests;

namespace Archdraw.Application.Modules.Components.Responses;

public sealed record ComponentResponse(
    Guid Id, Guid ProjectId, string TypeId, NodePropertiesDto Properties, int UsageCount,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
