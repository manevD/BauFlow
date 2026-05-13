using BauFlow.Data;
using BauFlow.Entities;
using Microsoft.EntityFrameworkCore;
namespace BauFlow.Services
{
    public class NumberService
    {
        private readonly ApplicationDbContext _context;

        public NumberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetNextInvoiceNumber(Guid companyId)
        {
            var entry = await _context.RunningNumbers
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    x.Type == "Invoice");

            if (entry == null)
            {
                entry = new RunningNumber
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    Type = "Invoice",
                    CurrentNumber = 1
                };

                _context.RunningNumbers.Add(entry);
            }
            else
            {
                entry.CurrentNumber++;
            }

            await _context.SaveChangesAsync();

            return $"Фактура-{DateTime.Now.Year}-{entry.CurrentNumber:D4}";
        }
    }
}
