using Archdraw.Presentation.Settings;
using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace Archdraw.Presentation.Swagger;

public static class SwaggerConfiguration
{
    public static void AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var api = configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();

        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = api.Title,
                Version = $"v{api.Version}",
                Description = api.Description
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o access token JWT."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] }
            });

            var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(SwaggerConfiguration).Assembly.GetName().Name}.xml");
            if (File.Exists(xml)) options.IncludeXmlComments(xml, includeControllerXmlComments: true);
        });

        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        }).AddMvc().AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}
