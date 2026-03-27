using System.ComponentModel.DataAnnotations;

namespace BauFlow.ViewModels
{
    public class CompanyEmailSettingsVM
    {
        public Guid Id { get; set; }

        public string EmailHost { get; set; }
        public int EmailPort { get; set; }
        public string EmailUser { get; set; }
        [DataType(DataType.Password)]
        public string EmailPassword { get; set; }
        public bool EmailSSL { get; set; }

        public string EmailFrom { get; set; }
        public string EmailFromName { get; set; }
    }
}
