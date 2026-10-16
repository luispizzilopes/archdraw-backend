using Archdraw.Presentation.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddConfigurations(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.AddRequestPipelineConfigurations();

app.Run();

public partial class Program;
