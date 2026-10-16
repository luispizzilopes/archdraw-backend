using Archdraw.Application.Modules.Auth.Commands.ChangePassword;
using Archdraw.Application.Modules.Auth.Commands.ConfirmPasswordReset;
using Archdraw.Application.Modules.Auth.Commands.DeleteProfilePhoto;
using Archdraw.Application.Modules.Auth.Commands.Login;
using Archdraw.Application.Modules.Auth.Commands.Logout;
using Archdraw.Application.Modules.Auth.Commands.RefreshSession;
using Archdraw.Application.Modules.Auth.Commands.Register;
using Archdraw.Application.Modules.Auth.Commands.RequestPasswordReset;
using Archdraw.Application.Modules.Auth.Commands.UpdateProfile;
using Archdraw.Application.Modules.Auth.Commands.UploadProfilePhoto;
using Archdraw.Application.Modules.Auth.Models;
using Archdraw.Application.Modules.Auth.Queries.GetProfile;
using Archdraw.Application.Modules.Auth.Requests;
using Archdraw.Application.Modules.Auth.Responses;
using Archdraw.Presentation.Extensions;
using Asp.Versioning;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Archdraw.Presentation.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController(IMediator mediator, IWebHostEnvironment environment, IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitExtension.AuthPolicy)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        => CompleteAuthentication(await mediator.Send(new RegisterCommand(request, HttpContext.GetSessionMetadata())));

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitExtension.AuthPolicy)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => CompleteAuthentication(await mediator.Send(new LoginCommand(request, HttpContext.GetSessionMetadata())));

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitExtension.RefreshPolicy)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh()
    {
        if (!HasAllowedOrigin()) return StatusCode(StatusCodes.Status403Forbidden);
        if (!Request.Cookies.TryGetValue(AuthSessionHttpExtensions.RefreshTokenCookieName, out var token) || string.IsNullOrWhiteSpace(token))
            return Unauthorized(new ApiError("Auth.MissingSession", "Sessão não encontrada.", "Unauthorized", [], null));

        var result = await mediator.Send(new RefreshSessionCommand(token, HttpContext.GetSessionMetadata()));
        if (result.IsError) Response.DeleteRefreshTokenCookie(environment);
        return CompleteAuthentication(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        if (!HasAllowedOrigin()) return StatusCode(StatusCodes.Status403Forbidden);
        Request.Cookies.TryGetValue(AuthSessionHttpExtensions.RefreshTokenCookieName, out var token);
        await mediator.Send(new LogoutCommand(token));
        Response.DeleteRefreshTokenCookie(environment);
        return NoContent();
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
        => (await mediator.Send(new GetProfileQuery(HttpContext.GetUserId()))).ToActionResult(this);

    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        => (await mediator.Send(new UpdateProfileCommand(HttpContext.GetUserId(), request))).ToActionResult(this);

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        => (await mediator.Send(new ChangePasswordCommand(HttpContext.GetUserId(), HttpContext.GetSessionId(), request))).ToActionResult(this);

    [HttpPost("password-reset/request")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitExtension.AuthPolicy)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        => (await mediator.Send(new RequestPasswordResetCommand(request))).ToActionResult(this);

    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitExtension.AuthPolicy)]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request)
        => (await mediator.Send(new ConfirmPasswordResetCommand(request))).ToActionResult(this);

    [HttpPost("profile/photo")]
    [Authorize]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var upload = new FileUpload(stream, file.FileName, file.ContentType, file.Length);
        var result = await mediator.Send(new UploadProfilePhotoCommand(HttpContext.GetUserId(), upload));
        return result.IsError ? result.ToActionResult(this) : Ok(new { profilePhotoUrl = result.Value });
    }

    [HttpDelete("profile/photo")]
    [Authorize]
    public async Task<IActionResult> DeleteProfilePhoto()
        => (await mediator.Send(new DeleteProfilePhotoCommand(HttpContext.GetUserId()))).ToActionResult(this);

    private IActionResult CompleteAuthentication(ErrorOr<LoginResponse> result)
    {
        if (result.IsError) return result.ToActionResult(this);
        Response.SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.RefreshTokenExpiresAtUtc, environment);
        return Ok(result.Value);
    }

    private bool HasAllowedOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        var allowed = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        return !string.IsNullOrEmpty(origin) && allowed.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
