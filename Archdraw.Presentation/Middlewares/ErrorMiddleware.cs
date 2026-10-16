using System.Text.Json;
using System.Text.Json.Serialization;
using Archdraw.Presentation.Extensions;

namespace Archdraw.Presentation.Middlewares;

public class ErrorMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ErrorMiddleware> logger)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
            var description = env.IsProduction() ? "Ocorreu um erro inesperado." : ex.InnerException?.Message ?? ex.Message;
            var body = new ApiError("InternalServerError", description, "Failure", [new("InternalServerError", description)], null);

            if (context.Response.HasStarted) throw;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(body, Options));
        }
    }
}
