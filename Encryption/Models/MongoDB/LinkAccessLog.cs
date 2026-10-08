namespace Encryption.Models.MongoDB
{
    public class LinkAccessLog
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string IpAddressHash { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public string? FailureReason{get; set; }
    }
}
