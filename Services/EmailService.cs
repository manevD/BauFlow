using BauFlow.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendInvite(string email, string name, string inviteLink)
    {
        var html = BuildInviteTemplate(name, inviteLink);

        var message = new MailMessage
        {
            From = new MailAddress(_settings.From, _settings.FromName),
            Subject = "Einladung zu BauFlow",
            Body = html,
            IsBodyHtml = true
        };

        message.To.Add(email);

        using var smtp = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
            EnableSsl = _settings.EnableSSL
        };

        await smtp.SendMailAsync(message);
    }

    private string BuildInviteTemplate(string name, string link)
    {
        return $"""
                        <!DOCTYPE html>
                        <html>
                        <head>
                        <meta charset="UTF-8">
                        </head>

                        <body style="margin:0;background:#f6f9fc;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;">

                        <table width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                        <td align="center">

                        <table width="520" cellpadding="0" cellspacing="0" style="margin-top:40px;background:white;border-radius:12px;padding:40px">

                        <tr>
                        <td style="font-size:24px;font-weight:600;color:#111">
                        BauFlow
                        </td>
                        </tr>

                        <tr>
                        <td style="padding-top:30px;font-size:18px;font-weight:500">
                        Du wurdest eingeladen
                        </td>
                        </tr>

                        <tr>
                        <td style="padding-top:10px;color:#555;font-size:15px;line-height:1.6">
                        {name},<br><br>
                        du wurdest zu <b>BauFlow</b> eingeladen.<br>
                        Klicke auf den Button, um dein Konto zu aktivieren.
                        </td>
                        </tr>

                        <tr>
                        <td align="center" style="padding-top:35px">

                        <a href="{link}"
                        style="
                        background:#635bff;
                        color:white;
                        text-decoration:none;
                        padding:14px 26px;
                        border-radius:8px;
                        font-weight:600;
                        display:inline-block;
                        font-size:15px;">
                        Konto aktivieren
                        </a>

                        </td>
                        </tr>

                        <tr>
                        <td style="padding-top:30px;color:#888;font-size:13px">
                        Oder kopiere diesen Link in deinen Browser:<br>
                        {link}
                        </td>
                        </tr>

                        <tr>
                        <td style="padding-top:30px;font-size:12px;color:#999">
                        Dieser Einladungslink läuft aus Sicherheitsgründen nach 24 Stunden ab.
                        </td>
                        </tr>

                        </table>

                        <table width="520" cellpadding="0" cellspacing="0" style="margin-top:15px">
                        <tr>
                        <td style="text-align:center;font-size:12px;color:#999">
                        © {DateTime.UtcNow.Year} BauFlow
                        </td>
                        </tr>
                        </table>

                        </td>
                        </tr>
                        </table>

                        </body>
                        </html>
                        """;
    }
}