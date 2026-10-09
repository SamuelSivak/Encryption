using System.ComponentModel.DataAnnotations;

namespace Encryption.DTO
{
    public class DecryptLinkRequestDto
    {
        [Required]
        public string Passphrase { get; set; } = string.Empty;
    }
}
