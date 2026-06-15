using BauFlow.Data;
using BauFlow.Entities;
using BauFlow.Interfaces;
using BauFlow.Models;
using BauFlow.Services;
using Microsoft.EntityFrameworkCore;
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

    public async Task SendInvite(
        string email,
        string name,
        string inviteLink)
    {
        var html = BuildInviteTemplate(name, inviteLink);

        using var message = new MailMessage
        {
            From = new MailAddress(
                _defaultSettings.From,
                _defaultSettings.FromName),

            Subject = "Покана од Флоу",

            Body = html,

            IsBodyHtml = true
        };

        message.To.Add(email);

        using var smtp = BuildSmtp(_defaultSettings);

        await smtp.SendMailAsync(message);
    }



    public async Task SendInvoice(
        string toEmail,
        Invoice invoice,
        EmailSettings settings,
        string companyName,
        bool sendToAccountant)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new Exception("Email receiver missing");


        // TEMPLATE
        var template = _templateService.LoadTemplate(
            sendToAccountant
            ? "InvoiceAccountant.cshtml"
            : "Invoice.cshtml");



        // COMPANY FOR PDF
        var company = await _context.Companies
            .FirstOrDefaultAsync(
                x => x.Id == invoice.CompanyId);


        if (company == null)
            throw new Exception(
                "Company missing for PDF");



        byte[] pdf;

        try
        {
            var document =
                new InvoiceDocument(
                    invoice,
                    company);

            pdf = document.GeneratePdf();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "PDF ERROR: " +
                ex.ToString());
        }


        var data =
            new Dictionary<string, string>
            {
                {
                    "CustomerName",
                    invoice.Customer?.Name ?? ""
                },

                {
                    "InvoiceNumber",
                    invoice.InvoiceNumber
                },

                {
                    "InvoiceDate",
                    invoice.InvoiceDate
                    .ToString("dd.MM.yyyy")
                },

                {
                    "Total",
                    invoice.GrossAmount
                    .ToString("0.00") + " МКД"
                },

                {
                    "Description",
                    invoice.Description ?? ""
                },

                {
                    "CompanyName",
                    companyName
                }
            };



        var body =
            _templateService
            .ReplacePlaceholders(
                template,
                data);




        using var message = new MailMessage
        {
            From =
                new MailAddress(
                    settings.From,
                    settings.FromName),

            Subject =
                $"Фактура {invoice.InvoiceNumber}",

            Body =
                body,

            IsBodyHtml =
                true
        };



        using var stream =
            new MemoryStream(pdf);



        message.Attachments.Add(
            new Attachment(
                stream,
                $"Invoice-{invoice.InvoiceNumber}.pdf",
                "application/pdf"));



        message.To.Add(toEmail);



        using var smtp =
            BuildSmtp(settings);



        await smtp.SendMailAsync(message);
    }



    private SmtpClient BuildSmtp(EmailSettings settings)
    {
        var smtp = new SmtpClient(settings.Host, settings.Port);

        smtp.EnableSsl = settings.EnableSSL;

        smtp.UseDefaultCredentials = false;

        smtp.Credentials =
            new NetworkCredential(
                settings.UserName,
                settings.Password);

        smtp.DeliveryMethod =
            SmtpDeliveryMethod.Network;

        return smtp;
    }




    private string BuildInviteTemplate(
        string name,
        string link)
    {
        return $"""
        <!DOCTYPE html>
        <html>
        <body>

        <h2>BauFlow</h2>

        <p>
        {name}, добивте покана.
        </p>

        <a href="{link}">
        Активирај сметка
        </a>

        <br/>

        {link}

        </body>
        </html>
        """;
    }
}