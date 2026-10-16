using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Archdraw.Presentation.Extensions;

public sealed record ApiErrorDetail(string Code, string Description);

public sealed record ApiError(string Code, string Description, string Type, IReadOnlyList<ApiErrorDetail> Errors, IDictionary<string, object>? Metadata);

public static class ActionResultExtension
{
    public static IActionResult ToActionResult<T>(this ErrorOr<T> result, ControllerBase controller)
        => result.IsError ? ToErrorResult(result.Errors, controller) : ToSuccess(result.Value, controller, StatusCodes.Status200OK);

    public static IActionResult ToCreatedResult<T>(this ErrorOr<T> result, ControllerBase controller)
        => result.IsError ? ToErrorResult(result.Errors, controller) : ToSuccess(result.Value, controller, StatusCodes.Status201Created);

    private static IActionResult ToSuccess<T>(T? value, ControllerBase controller, int statusCode)
    {
        if (value is null or Success) return controller.NoContent();
        return statusCode == StatusCodes.Status200OK ? controller.Ok(value) : controller.StatusCode(statusCode, value);
    }

    public static IActionResult ToErrorResult(IReadOnlyList<Error> errors, ControllerBase controller)
    {
        var first = errors[0];
        var body = new ApiError(
            first.Code, first.Description, first.Type.ToString(),
            errors.Select(e => new ApiErrorDetail(e.Code, e.Description)).ToList(),
            first.Metadata);

        return first.Type switch
        {
            ErrorType.Validation => controller.BadRequest(body),
            ErrorType.NotFound => controller.NotFound(body),
            ErrorType.Unauthorized => controller.Unauthorized(body),
            ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, body),
            ErrorType.Conflict => controller.Conflict(body),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError, body)
        };
    }
}
