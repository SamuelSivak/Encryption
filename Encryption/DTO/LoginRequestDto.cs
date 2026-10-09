using System.ComponentModel.DataAnnotations;

namespace Encryption.DTO
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Heslo je povinné")]
        public string Password { get; set; } = string.Empty;
    }
}
