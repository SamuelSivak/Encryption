using Encryption.Data;
using Encryption.DTO;
using Encryption.Interfaces;
using Encryption.Models.MongoDB;
using Encryption.Models.SQL;
using Encryption.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Security.Cryptography;

namespace Encryption.Services;

public class LinkService : ILinkService
{
    private readonly AppDbContext _sqlDbContext;
    private readonly AppReadDbContext _readDbContext;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly IEncryptionService _encryptionService;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LinkService> _logger;

    public LinkService(
        AppDbContext sqlDbContext,
        AppReadDbContext readDbContext,
        IMongoDbContext mongoDbContext,
        IEncryptionService encryptionService,
        ICacheService cacheService,
        IConfiguration configuration,
        ILogger<LinkService> logger)
    {
        _sqlDbContext = sqlDbContext;
        _readDbContext = readDbContext;
        _mongoDbContext = mongoDbContext;
        _encryptionService = encryptionService;
        _cacheService = cacheService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CreateLinkResponseDto> CreateEncryptedLinkAsync(CreateLinkRequestDto dto, string ownerUserId)
    {
        string shortCode = await GenerateShortCode();

        var encResult = _encryptionService.Encrypt(dto.OriginalUrl, dto.Passphrase);

        DateTime? expiresAt = dto.ExpiresInMinutes.HasValue && dto.ExpiresInMinutes.Value > 0
            ? DateTime.UtcNow.AddMinutes(dto.ExpiresInMinutes.Value)
            : null;

        var meta = new EncryptedLinkMeta
        {
            ShortCode = shortCode,
            BlindIndexHash = encResult.BlindIndexHash,
            OwnerUserId = ownerUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            MaxAccessCount = dto.MaxAccessCount ?? 0,
            BurnOnRead = dto.BurnOnRead,
            IsActive = true
        };

        _sqlDbContext.EncryptedLinkMetas.Add(meta);
        await _sqlDbContext.SaveChangesAsync();

        var doc = new EncryptedLinkDocument
        {
            ShortCode = shortCode,
            UserId = ownerUserId,
            EncryptedUrlPayload = encResult.CiphertextBase64,
            IV = encResult.IvBase64,
            Salt = encResult.SaltBase64,
            AuthTag = encResult.AuthTagBase64,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _mongoDbContext.EncryptedLinks.InsertOneAsync(doc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert document into MongoDB. Rolling back SQL metadata for shortCode: {ShortCode}", shortCode);
            _sqlDbContext.EncryptedLinkMetas.Remove(meta);
            await _sqlDbContext.SaveChangesAsync();
            throw;
        }
        return new CreateLinkResponseDto
        {
            ShortCode = shortCode,
            FullUrl = $"/api/links/{shortCode}",
            ExpiresAt = expiresAt
        };
    }

    public async Task<DecryptLinkResponseDto> DecryptLinkAsync(string shortCode, DecryptLinkRequestDto dto, string clientIp, string userAgent)
    {

        string rateLimitKey = $"ratelimit:decrypt:{clientIp}:{shortCode}";
        bool isAllowed = await _cacheService.IncrementRateLimitAsync(rateLimitKey, TimeSpan.FromMinutes(2), 5);

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for IP {ClientIp} on link {ShortCode}", clientIp, shortCode);
            throw new InvalidOperationException("Too many decrypt attempts. Please try again later.");
        }

        var meta = await _sqlDbContext.EncryptedLinkMetas
            .FirstOrDefaultAsync(l => l.ShortCode == shortCode && l.IsActive);

        if (meta == null)
            throw new KeyNotFoundException("Link was not found or is no longer active.");

        if (meta.ExpiresAt.HasValue && meta.ExpiresAt.Value < DateTime.UtcNow)
        {
            meta.IsActive = false;
            await _sqlDbContext.SaveChangesAsync();
            throw new InvalidOperationException("This link has expired.");
        }

        if (meta.MaxAccessCount > 0 && meta.CurrentAccessCount >= meta.MaxAccessCount)
        {
            meta.IsActive = false;
            await _sqlDbContext.SaveChangesAsync();
            throw new InvalidOperationException("The maximum access count for this link has been reached.");
        }

        string cacheKey = $"link_doc:{shortCode}";
        var doc = await _cacheService.GetAsync<EncryptedLinkDocument>(cacheKey);

        if (doc == null)
        {
            doc = await _mongoDbContext.EncryptedLinks
                .Find(d => d.ShortCode == shortCode)
                .FirstOrDefaultAsync();

            if (doc == null)
                throw new KeyNotFoundException("Encrypted content was not found.");

            await _cacheService.SetAsync(cacheKey, doc, TimeSpan.FromMinutes(5));
        }

        string decryptedUrl;
        string? failureReason = null;

        try
        {
            decryptedUrl = _encryptionService.Decrypt(
                doc.EncryptedUrlPayload,
                doc.IV,
                doc.Salt,
                doc.AuthTag,
                dto.Passphrase);
        }
        catch (Exception ex)
        { 
            failureReason = ex.Message;

            await LogAccessAttemptAsync(shortCode, clientIp, userAgent, false, failureReason);

            throw;
        }

        meta.CurrentAccessCount++;
        bool isBurned = false;

        if (meta.BurnOnRead || (meta.MaxAccessCount > 0 && meta.CurrentAccessCount >= meta.MaxAccessCount))
        {
            meta.IsActive = false;
            isBurned = true;
            await _cacheService.RemoveAsync(cacheKey);
        }
        try
        {
            await _sqlDbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict detected while updating link {ShortCode}", shortCode);
            throw new InvalidOperationException("This link was accessed or burned by another concurrent request.");
        }

        await LogAccessAttemptAsync(shortCode, clientIp, userAgent, true, null);

        int remaining = meta.MaxAccessCount > 0 ? meta.MaxAccessCount - meta.CurrentAccessCount : -1;

        return new DecryptLinkResponseDto
        {
            OriginalUrl = decryptedUrl,
            RemainingAccesses = remaining,
            isBurned = isBurned
        };
    }

    public async Task<IEnumerable<CreateLinkResponseDto>> GetUserLinksAsync(string userId)
    {
        var links = await _readDbContext.EncryptedLinkMetas
            .Where(l => l.OwnerUserId == userId && l.IsActive)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new CreateLinkResponseDto
            {
                ShortCode = l.ShortCode, 
                FullUrl = $"/api/links/{l.ShortCode}",
                ExpiresAt = l.ExpiresAt
            })
            .ToListAsync();

        return links;
    }

    private async Task LogAccessAttemptAsync(string shortCode, string clientIp, string userAgent, bool success, string? failureReason)
    {
        string ippepper = _configuration["IpHashPepper"] ?? "DefaultIpPepperSecretKey_ChangeInProd!";
        var filter = Builders<EncryptedLinkDocument>.Filter.Eq(d => d.ShortCode, shortCode);
        var update = Builders<EncryptedLinkDocument>.Update.Push(d => d.AccessLogs, new LinkAccessLog
        {
            Timestamp = DateTime.UtcNow,
            IpAddressHash = _encryptionService.GenerateBlindIndex(clientIp, ippepper),
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason
        });
        await _mongoDbContext.EncryptedLinks.UpdateOneAsync(filter, update);
    }

    private async Task<string> GenerateShortCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        

        for(int i = 0; i < 5; i++)
        {
            string code = RandomNumberGenerator.GetString(chars, 7);
            bool exists = await _readDbContext.EncryptedLinkMetas.AnyAsync(l => l.ShortCode == code);
            if(!exists)
            {
                return code;
            }
        }
        throw new InvalidOperationException("Failed to generate a unique short code.");
    }
}