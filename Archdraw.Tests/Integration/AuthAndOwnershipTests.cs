using System.Net;
using System.Net.Http.Json;

namespace Archdraw.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthAndOwnershipTests(ApiFixture api)
{
    [Fact]
    public async Task Endpoints_exigem_autenticacao()
    {
        var anonymous = api.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/v1/projects")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync($"/api/v1/diagrams/{Guid.NewGuid()}/snapshot")).StatusCode);
    }

    [Fact]
    public async Task Cadastro_rejeita_senha_fraca_e_email_duplicado()
    {
        var client = api.CreateClient();
        var weak = await client.PostAsJsonAsync("/api/v1/authentication/register", new { name = "A", email = "fraca@teste.com", password = "abc" });
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);

        var user = await api.CreateUserAsync();
        var duplicate = await client.PostAsJsonAsync("/api/v1/authentication/register", new { name = "B", email = user.Email, password = "Senha1234" });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
    }

    [Fact]
    public async Task Login_retorna_token_e_rejeita_credenciais_invalidas()
    {
        var user = await api.CreateUserAsync();
        var client = api.CreateClient();

        var ok = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = user.Email, password = "Senha1234" });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Contains("token", await ok.Content.ReadAsStringAsync());

        var bad = await client.PostAsJsonAsync("/api/v1/authentication/login", new { email = user.Email, password = "errada" });
        Assert.Equal(HttpStatusCode.Unauthorized, bad.StatusCode);
    }

    [Fact]
    public async Task Perfil_e_atualizado_para_o_usuario_autenticado()
    {
        var user = await api.CreateUserAsync("Nome Antigo");
        var update = await user.Client.PutAsJsonAsync("/api/v1/authentication/profile", new { name = "Nome Novo" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var profile = await (await user.Client.GetAsync("/api/v1/authentication/profile")).ReadAsync();
        Assert.Equal("Nome Novo", profile.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Troca_de_senha_exige_senha_atual()
    {
        var user = await api.CreateUserAsync();
        var wrong = await user.Client.PutAsJsonAsync("/api/v1/authentication/password", new { currentPassword = "Errada123", newPassword = "NovaSenha123" });
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);

        var ok = await user.Client.PutAsJsonAsync("/api/v1/authentication/password", new { currentPassword = "Senha1234", newPassword = "NovaSenha123" });
        Assert.Equal(HttpStatusCode.NoContent, ok.StatusCode);

        var login = await api.CreateClient().PostAsJsonAsync("/api/v1/authentication/login", new { email = user.Email, password = "NovaSenha123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Usuario_so_ve_os_proprios_projetos()
    {
        var alice = await api.CreateUserAsync();
        var bob = await api.CreateUserAsync();
        var aliceProject = await alice.Client.CreateProjectAsync("Da Alice");
        await bob.Client.CreateProjectAsync("Do Bob");

        var bobList = await (await bob.Client.GetAsync("/api/v1/projects")).ReadAsync();
        var names = bobList.GetProperty("items").EnumerateArray().Select(p => p.GetProperty("name").GetString()).ToList();
        Assert.Equal(["Do Bob"], names);

        Assert.Equal(HttpStatusCode.NotFound, (await bob.Client.GetAsync($"/api/v1/projects/{aliceProject}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.Client.DeleteAsync($"/api/v1/projects/{aliceProject}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await alice.Client.GetAsync($"/api/v1/projects/{aliceProject}")).StatusCode);
    }

    [Fact]
    public async Task Recursos_de_outro_usuario_sao_inacessiveis_em_todos_os_modulos()
    {
        var alice = await api.CreateUserAsync();
        var bob = await api.CreateUserAsync();
        var projectId = await alice.Client.CreateProjectAsync();
        var diagramId = await alice.Client.CreateDiagramAsync(projectId);
        var componentId = await alice.Client.CreateComponentAsync(projectId);

        var b = bob.Client;
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/v1/projects/{projectId}/diagrams")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/v1/diagrams/{diagramId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/v1/diagrams/{diagramId}/snapshot")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/v1/diagrams/{diagramId}/documentation")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PutAsJsonAsync($"/api/v1/diagrams/{diagramId}/documentation", new { markdown = "x", baseRevision = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/v1/diagrams/{diagramId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PostAsJsonAsync($"/api/v1/diagrams/{diagramId}/duplicate", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/v1/components/{componentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/v1/components/{componentId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.PostAsJsonAsync($"/api/v1/projects/{projectId}/diagrams",
            new { name = "Invasor", level = "free" })).StatusCode);
    }
}
