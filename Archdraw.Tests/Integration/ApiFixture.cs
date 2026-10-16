using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Archdraw.Tests.Integration;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().WithImage("postgres:16").Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var settings = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
            ["Jwt:key"] = "TESTS_ONLY_SECRET_KEY_WITH_MORE_THAN_32_CHARS",
            ["Database:MigrateOnStartup"] = "true",
            ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
            ["RateLimit:AuthPermitPerMinute"] = "100000",
            ["RateLimit:RefreshPermitPerMinute"] = "100000",
            ["LocalStorage:RootPath"] = Path.Combine(Path.GetTempPath(), "archdraw-tests-uploads")
        };
        foreach (var (key, value) in settings) builder.UseSetting(key, value);
    }

    public async Task<TestUser> CreateUserAsync(string? name = null)
    {
        var client = CreateClient();
        var email = $"u{Guid.NewGuid():N}@teste.com";
        var response = await client.PostAsJsonAsync("/api/v1/authentication/register",
            new { name = name ?? "Usuário Teste", email, password = "Senha1234" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginBody>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return new TestUser(client, body.UserId, email);
    }

    public sealed record LoginBody(Guid UserId, string Token);
}

public sealed record TestUser(HttpClient Client, Guid Id, string Email);

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "api";
}
