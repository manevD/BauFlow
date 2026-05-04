using System.ComponentModel.DataAnnotations;

namespace BauFlow.ViewModels
{
    public class CompanyViewModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

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
        [Required(ErrorMessage = "IBAN е задолжителен")]
        [MaxLength(34)]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]{1,30}$",
           ErrorMessage = "Невалиден IBAN формат")]
        public string IBAN { get; set; }
    }
}
