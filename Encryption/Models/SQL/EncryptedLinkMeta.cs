using System.ComponentModel.DataAnnotations;

namespace Encryption.Models.SQL
{
    public class EncryptedLinkMeta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(16)]
        public string ShortCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string BlindIndexHash { get; set; } = string.Empty;

        [Required]
        public string OwnerUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }
        
        public int MaxAccessCount { get; set; } = 0;
        public int CurrentAccessCount { get; set; } = 0;

        public bool BurnOnRead { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public byte[]? Rowversion { get; set; }

    }
}
