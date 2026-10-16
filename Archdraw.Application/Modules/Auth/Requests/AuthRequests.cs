namespace Archdraw.Application.Modules.Auth.Requests;

public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record UpdateProfileRequest(string Name);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record RequestPasswordResetRequest(string Email);
public sealed record ConfirmPasswordResetRequest(string Email, string Code, string NewPassword);
