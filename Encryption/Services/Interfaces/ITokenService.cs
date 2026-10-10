using Encryption.Models.SQL;

namespace Encryption.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);

    }
}
