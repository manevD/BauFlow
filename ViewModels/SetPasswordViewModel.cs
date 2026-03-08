using System.ComponentModel.DataAnnotations;

namespace BauFlow.ViewModels
{
    public class SetPasswordViewModel
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
