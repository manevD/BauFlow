using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Interfaces;
using BauFlow.Models;
using BauFlow.Services;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailTemplateService _templateService;
    private readonly EmailSettings _defaultSettings;
    private readonly ApplicationDbContext _context;

    public EmailService(
        EmailTemplateService templateService,
        IOptions<EmailSettings> defaultSettings,
        ApplicationDbContext context)
    {
        _templateService = templateService;
        _defaultSettings = defaultSettings.Value;
        _context = context;
    }
    public async Task SendInvite(string email, string name, string inviteLink)
    {
        var html = BuildInviteTemplate(name, inviteLink);

        var message = new MailMessage
        {
            From = new MailAddress(_defaultSettings.From, _defaultSettings.FromName),
            Subject = "Покана од Флоу",
            Body = html,
            IsBodyHtml = true
        };

        message.To.Add(email);
        using var smtp = new SmtpClient(_defaultSettings.Host, _defaultSettings.Port)
        {
            Credentials = new NetworkCredential(_defaultSettings.UserName, _defaultSettings.Password),
            EnableSsl = _defaultSettings.EnableSSL
        };

        await smtp.SendMailAsync(message);
    }

    public async Task SendInvoice(string toEmail, Invoice invoice, EmailSettings settings,string companyName)
    {
        var template = _templateService.LoadTemplate("Invoice.cshtml");
        var document = new InvoiceDocument(invoice, _context.Companies.Find(_context.CurrentCompanyId.Value));
        var pdf = document.GeneratePdf();

        var data = new Dictionary<string, string>
        {
            { "CustomerName", invoice.Customer?.Name ?? "" },
            { "InvoiceNumber", invoice.InvoiceNumber },
            { "InvoiceDate", invoice.InvoiceDate.ToString("dd.MM.yyyy") },
            { "Total", invoice.GrossAmount.ToString() + " МКД"},
            { "Description", invoice.Description ?? "" }
        };

        var body = _templateService.ReplacePlaceholders(template, data);

        var message = new MailMessage
        {
            From = new MailAddress(settings.From, settings.FromName),
            Subject = $"Фактура {invoice.InvoiceNumber}",
            Body = body,
            Attachments = { new Attachment(new MemoryStream(pdf), $"Фактура{invoice.InvoiceNumber}.pdf") },
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using var smtp = BuildSmtp(settings);
        await smtp.SendMailAsync(message);
    }

    private SmtpClient BuildSmtp(EmailSettings settings)
    {
        return new SmtpClient(settings.Host, settings.Port)
        {
            Credentials = new NetworkCredential(settings.UserName, settings.Password),
            EnableSsl = settings.EnableSSL
        };
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
                    Добивте покана
                    </td>
                    </tr>

                    <tr>
                    <td style="padding-top:10px;color:#555;font-size:15px;line-height:1.6">
                    {name},<br><br>
                    поканети сте во <b>BauFlow</b>.<br>
                    Кликнете на копчето подолу за да ја активирате вашата сметка.
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
                    Активирај сметка
                    </a>

                    </td>
                    </tr>

                    <tr>
                    <td style="padding-top:30px;color:#888;font-size:13px">
                    Или копирајте го следниот линк во вашиот прелистувач:<br>
                    {link}
                    </td>
                    </tr>

                    <tr>
                    <td style="padding-top:30px;font-size:12px;color:#999">
                    Од безбедносни причини, овој линк за покана истекува по 24 часа.
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