using BauFlow.Entities;
using BauFlow.Models;

namespace BauFlow.Interfaces
{
    public interface IEmailService
    {
        Task SendInvoice(string toEmail, Invoice invoice, EmailSettings settings);
    }
}
