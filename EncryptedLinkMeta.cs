using Microsoft.ComponentModel.DataAnnotations;

namespace Encryption.Models.SQL;

public class EncryptedLinkMeta
{
    [Key]
    public Guild Id { get; set; } = Guild.NewGuid();

    [Required]
    [MaxLength(16)]
    public string ShortCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string BlindIndexHash { get; set; } = string.Empty;

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    public Datetime CreatedAt { get; set; } = Datetime.UtcNow;
    public Datetime? ExpireAt { get; set; }
    public int MaxAccessCount { get; set; } = 0;
    public int CurrentAccessCount { get; set; } = 0;
    public bool BurnAfterRead { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
}