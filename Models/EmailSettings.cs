using System.ComponentModel.DataAnnotations;

namespace BauFlow.Models
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool EnableSSL { get; set; }

        public string From { get; set; }
        public string FromName { get; set; }
    }
}
