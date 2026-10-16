namespace Archdraw.Application.Common.Emails;

public static class EmailTemplates
{
    public static class Subjects
    {
        public const string PasswordReset = "Archdraw — código para redefinir sua senha";
    }

    public static string PasswordReset(string code) => $"""
        <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto">
          <h2 style="margin:0 0 12px">Redefinir senha</h2>
          <p>Use o código abaixo para redefinir a senha da sua conta Archdraw. Ele expira em 15 minutos.</p>
          <p style="font-size:32px;letter-spacing:6px;font-weight:bold;margin:24px 0">{code}</p>
          <p style="color:#666;font-size:13px">Se você não solicitou, ignore este e-mail.</p>
        </div>
        """;
}
