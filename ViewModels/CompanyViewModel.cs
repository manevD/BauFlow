using System.ComponentModel.DataAnnotations;

namespace BauFlow.ViewModels
{
    public class CompanyViewModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [EmailAddress]
        public string? Accountant { get; set; }
        // ADDRESS
        [Required, MaxLength(300)]
        public string Address { get; set; }

        [Required, MaxLength(20)]
        public string PostalCode { get; set; }

        [Required, MaxLength(150)]
        public string City { get; set; }

        [Required, MaxLength(150)]
        public string Country { get; set; }

        // TAX
        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [Required(ErrorMessage = "Жиросметката е задолжителна")]
        public string IBAN { get; set; }

        [Required(ErrorMessage = "Име на банката е задолжително")]
        public string BankName { get; set; }

    }
}
