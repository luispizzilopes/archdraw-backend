namespace Archdraw.BuildingBlocks.Common.Interfaces;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    DateTime UpdatedAtUtc { get; }

    void SetCreatedAt(DateTime timestampUtc);
    void SetUpdatedAt(DateTime timestampUtc);
}
