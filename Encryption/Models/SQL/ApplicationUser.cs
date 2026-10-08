using Microsoft.AspNetCore.Identity;

namespace Encryption.Models.SQL
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}
