using Encryption.DTO;

namespace Encryption.Services.Interfaces;

public interface ILinkService
{
 
    Task<CreateLinkResponseDto> CreateEncryptedLinkAsync(CreateLinkRequestDto dto, string ownerUserId);

    Task<DecryptLinkResponseDto> DecryptLinkAsync(string shortCode, DecryptLinkRequestDto dto, string clientIp, string userAgent);

    Task<IEnumerable<CreateLinkResponseDto>> GetUserLinksAsync(string userId);
}