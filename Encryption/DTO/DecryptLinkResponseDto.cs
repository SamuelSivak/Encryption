namespace Encryption.DTO
{
    public class DecryptLinkResponseDto
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public int? RemainingAccesses { get; set; }
        public bool isBurned { get; set; } = false;

    }
}
