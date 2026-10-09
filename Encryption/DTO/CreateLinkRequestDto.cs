using System.ComponentModel.DataAnnotations;

namespace Encryption.DTO
{
    public class CreateLinkRequestDto
    {
        [Required]
        [Url]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Passphrase { get; set; } = string.Empty;

        [Range(1, 525600)]
        public int? ExpiresInMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxAccessCount { get; set; }

        public bool BurnOnRead { get; set; } = false;

    }
}
