using Archdraw.BuildingBlocks.Common.Class.Base;

namespace Archdraw.Domain.Modules.Users.Entities;

public class UserSession : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastUsedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    private UserSession() { }

    public static UserSession Create(
        Guid userId, string refreshTokenHash, string deviceName, string? userAgent,
        string? ipAddress, DateTime expiresAtUtc, Guid? familyId = null)
    {
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            DeviceName = string.IsNullOrWhiteSpace(deviceName) ? "Dispositivo desconhecido" : deviceName.Trim(),
            UserAgent = userAgent,
            IpAddress = ipAddress,
            CreatedAtUtc = now,
            LastUsedAtUtc = now,
            ExpiresAtUtc = expiresAtUtc
        };
        session.FamilyId = familyId ?? session.Id;
        return session;
    }

    public void Rotate(Guid replacementSessionId)
        => Revoke("Rotação do token de atualização.", replacementSessionId);

    public void Revoke(string reason, Guid? replacementSessionId = null)
    {
        if (RevokedAtUtc is not null) return;
        RevokedAtUtc = DateTime.UtcNow;
        RevocationReason = reason;
        ReplacedBySessionId = replacementSessionId;
    }

    public void Touch(string? ipAddress)
    {
        LastUsedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(ipAddress)) IpAddress = ipAddress;
    }
}
