namespace Archdraw.Application.Modules.Auth.Models;

public sealed record SessionMetadata(string DeviceName, string? UserAgent, string? IpAddress);
