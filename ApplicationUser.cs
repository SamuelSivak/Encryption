using Microsoft.AspNetCore.Identity;

namespace Encryption.Models.SQL;

public class ApplicationUSer : IdentityUSer
{
    public Datetime CreatedAt { get; set; } = Datetime.UtcNow;
    public Datetime LastLoginAt { get; set; }

}

}