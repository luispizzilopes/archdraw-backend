using Archdraw.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Archdraw.Infrastructure.Services;

public class LogEmailService(ILogger<LogEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("E-mail (Brevo não configurado) para {To} — {Subject}: {Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
