using Microsoft.AspNetCore.DataProtection;

namespace BauFlow.Services
{
    public class EmailEncryptionService
    {
        private readonly IDataProtector _protector;

        public EmailEncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("EmailSettings.Password");
        }

        public string Encrypt(string input)
        {
            return _protector.Protect(input);
        }

        public string Decrypt(string input)
        {
            return _protector.Unprotect(input);
        }
    }
}
