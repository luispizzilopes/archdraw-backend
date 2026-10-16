using Archdraw.Application.Modules.Diagrams.Requests;

namespace Archdraw.Application.Modules.Components.Requests;

public sealed record CreateComponentRequest(Guid? Id, string TypeId, NodePropertiesDto Properties);
public sealed record UpdateComponentRequest(string TypeId, NodePropertiesDto Properties);
