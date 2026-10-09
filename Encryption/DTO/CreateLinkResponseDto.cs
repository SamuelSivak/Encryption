namespace Encryption.DTO
{
    public class CreateLinkResponseDto
    {
        public string ShortCode { get; set; } = string.Empty;
        public string FullUrl { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
