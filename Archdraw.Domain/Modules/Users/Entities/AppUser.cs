using Archdraw.BuildingBlocks.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Archdraw.Domain.Modules.Users.Entities;

public class AppUser : IdentityUser<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfilePhotoKey { get; set; }
    public int LockoutLevel { get; set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
