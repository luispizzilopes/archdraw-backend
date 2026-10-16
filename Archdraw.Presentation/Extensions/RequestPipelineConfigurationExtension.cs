using Archdraw.Infrastructure.Context;
using Archdraw.Infrastructure.Settings;
using Archdraw.Presentation.Middlewares;
using Asp.Versioning.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Archdraw.Presentation.Extensions;

public static class RequestPipelineConfigurationExtension
{
    public static async Task AddRequestPipelineConfigurations(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            using var scope = app.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        }

        app.UseMiddleware<ErrorMiddleware>();
        app.ConfigureSwagger();

        var origins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        app.UseCors(c => c.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

        if (!app.Environment.IsEnvironment("Localhost") && !app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseLocalUploads();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
    }

    private static void UseLocalUploads(this WebApplication app)
    {
        var local = app.Services.GetRequiredService<IOptions<LocalStorageSettings>>().Value;
        var root = Path.GetFullPath(local.RootPath);
        Directory.CreateDirectory(root);
        app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(root), RequestPath = "/uploads" });
    }

    private static void ConfigureSwagger(this WebApplication app)
    {
        if (app.Environment.IsProduction())
        {
            var username = app.Configuration["Swagger:Username"] ?? "swagger";
            var password = app.Configuration["Swagger:Password"]!;
            app.UseWhen(c => c.Request.Path.StartsWithSegments("/swagger"),
                branch => branch.UseMiddleware<SwaggerBasicAuthMiddleware>(username, password));
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var d in provider.ApiVersionDescriptions)
                options.SwaggerEndpoint($"/swagger/{d.GroupName}/swagger.json", d.GroupName.ToUpperInvariant());
            options.DefaultModelsExpandDepth(-1);
            options.RoutePrefix = "swagger";
        });
    }
}
