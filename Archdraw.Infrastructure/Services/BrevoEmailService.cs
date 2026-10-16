using System.Net.Http.Json;
using Archdraw.Domain.Interfaces.Services;
using Archdraw.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Archdraw.Infrastructure.Services;

public class BrevoEmailService(IHttpClientFactory httpClientFactory, IOptions<BrevoSettings> settings) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var s = settings.Value;
        var payload = new
        {
            sender = new { name = s.FromName, email = s.FromEmail },
            to = new[] { new { email = to } },
            subject,
            htmlContent = htmlBody
        };

        var response = await httpClientFactory.CreateClient("Brevo").PostAsJsonAsync("smtp/email", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
