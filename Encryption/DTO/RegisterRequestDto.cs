using System.ComponentModel.DataAnnotations;

namespace Encryption.DTO
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Heslo je povinné")]
        [MinLength(8, ErrorMessage = "Heslo musí mať minimálne 8 znakov")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrdenie hesla je povinné")]
        [Compare("Password", ErrorMessage = "Heslá sa nezhodujú")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
