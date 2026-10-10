namespace Encryption.Models.Configuration
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;

        public int ExpirationHours { get; set; } = 6;
    }
}
