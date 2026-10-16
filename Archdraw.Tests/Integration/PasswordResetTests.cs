using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Archdraw.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Archdraw.Tests.Integration;

[Collection(ApiCollection.Name)]
public partial class PasswordResetTests(ApiFixture api)
{
    private sealed class CapturingEmailService : IEmailService
    {
        public List<(string To, string Body)> Sent { get; } = [];
        public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            lock (Sent) Sent.Add((to, htmlBody));
            return Task.CompletedTask;
        }
    }

    [GeneratedRegex(@"\b\d{6}\b")]
    private static partial Regex CodeRegex();

    private (HttpClient Client, CapturingEmailService Emails) CreateClientWithEmailCapture()
    {
        var emails = new CapturingEmailService();
        var factory = api.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddSingleton<IEmailService>(emails)));
        return (factory.CreateClient(), emails);
    }

    [Fact]
    public async Task Recuperacao_de_senha_por_codigo_redefine_a_senha_e_encerra_sessoes()
    {
        var user = await api.CreateUserAsync();
        var (client, emails) = CreateClientWithEmailCapture();

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/v1/authentication/password-reset/request", new { email = user.Email })).StatusCode);
        var code = CodeRegex().Match(Assert.Single(emails.Sent).Body).Value;

        var confirm = await client.PostAsJsonAsync("/api/v1/authentication/password-reset/confirm", new { email = user.Email, code, newPassword = "OutraSenha99" });
        Assert.Equal(HttpStatusCode.NoContent, confirm.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = user.Email, password = "OutraSenha99" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = user.Email, password = "Senha1234" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await user.Client.GetAsync("/api/v1/projects")).StatusCode);
        var reuse = await client.PostAsJsonAsync("/api/v1/authentication/password-reset/confirm", new { email = user.Email, code, newPassword = "MaisUma1234" });
        Assert.Equal(HttpStatusCode.BadRequest, reuse.StatusCode);
    }

    [Fact]
    public async Task Solicitar_reset_para_email_inexistente_nao_revela_nada()
    {
        var (client, emails) = CreateClientWithEmailCapture();
        var response = await client.PostAsJsonAsync("/api/v1/authentication/password-reset/request", new { email = "naoexiste@teste.com" });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(emails.Sent);
    }

    [Fact]
    public async Task Codigo_e_invalidado_apos_tentativas_erradas()
    {
        var user = await api.CreateUserAsync();
        var (client, emails) = CreateClientWithEmailCapture();
        await client.PostAsJsonAsync("/api/v1/authentication/password-reset/request", new { email = user.Email });
        var code = CodeRegex().Match(emails.Sent.Single().Body).Value;
        var wrong = code == "000000" ? "111111" : "000000";

        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/authentication/password-reset/confirm", new { email = user.Email, code = wrong, newPassword = "OutraSenha99" })).StatusCode);

        var correctButBurned = await client.PostAsJsonAsync("/api/v1/authentication/password-reset/confirm", new { email = user.Email, code, newPassword = "OutraSenha99" });
        Assert.Equal(HttpStatusCode.BadRequest, correctButBurned.StatusCode);
    }

    [Fact]
    public async Task Foto_de_perfil_e_enviada_validada_e_removida()
    {
        var user = await api.CreateUserAsync();
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3 };

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(png);
        file.Headers.ContentType = new("image/png");
        form.Add(file, "file", "foto.png");
        var upload = await user.Client.PostAsync("/api/v1/authentication/profile/photo", form);
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        Assert.Contains("profile-photos", (await upload.ReadAsync()).GetProperty("profilePhotoUrl").GetString());

        using var bad = new MultipartFormDataContent();
        var text = new ByteArrayContent("não sou imagem"u8.ToArray());
        text.Headers.ContentType = new("text/plain");
        bad.Add(text, "file", "x.txt");
        Assert.Equal(HttpStatusCode.BadRequest, (await user.Client.PostAsync("/api/v1/authentication/profile/photo", bad)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await user.Client.DeleteAsync("/api/v1/authentication/profile/photo")).StatusCode);
    }
}
