using System.Text;
using System.Security.Cryptography;
using Encryption.Services.Interfaces;
using Konscious.Security.Cryptography;

namespace Encryption.Services;

public class EncryptionService : IEncryptionService
{
    private const int KeySizeBytes = 32; // 256 bits
    private const int SaltSizeBytes = 16; // 128 bits
    private const int IvSizeBytes = 12; // 96 bits for AES-GCM
    private const int TagSizeBytes = 16; // 128 bits for AES-GCM
    private const int Argon2Iterations = 3;
    private const int Argon2MemorySizeKb = 65536;
    private const int Argon2DegreeOfParallelism = 4;

    private readonly IConfiguration _configuration;

    public EncryptionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public EncryptionResult Encrypt(string plaiUrl, string passphrase)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        byte[] iv = RandomNumberGenerator.GetBytes(IvSizeBytes);

        byte[] key = DeriveKey(passphrase, salt);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plaiUrl);
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSizeBytes];

        using (var aesGcm = new AesGcm(key, TagSizeBytes))
        {
            aesGcm.Encrypt(iv, plainBytes, cipherBytes, tag);
        }

        string pepper = _configuration["Encryption:PepperSecret"] ?? "DefaultSecretPepperKey_ChangeInProd_123!";
        string blindIndex = GenerateBlindIndex(plaiUrl, pepper);

        return new EncryptionResult(
            CiphertextBase64: Convert.ToBase64String(cipherBytes),
            IvBase64: Convert.ToBase64String(iv),
            SaltBase64: Convert.ToBase64String(salt),
            AuthTagBase64: Convert.ToBase64String(tag),
            BlindIndexHash: blindIndex
        );
    }

    public string Decrypt(
        string cipherBase64, 
        string ivbase64, 
        string saltbase64, 
        string authBase64, 
        string passphrase)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherBase64);
        byte[] iv = Convert.FromBase64String(ivbase64);
        byte[] salt = Convert.FromBase64String(saltbase64);
        byte[] tag = Convert.FromBase64String(authBase64);
        byte[] key = DeriveKey(passphrase, salt);
        byte[] plainBytes = new byte[cipherBytes.Length];
        try
        {
            using (var aesGcm = new AesGcm(key, TagSizeBytes))
            aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);
            return Encoding.UTF8.GetString(plainBytes);
        } 
        catch (CryptographicException)
        {
            throw new InvalidOperationException("Decryption failed. Invalid passphrase or corrupted data.");
        }
    }

    public string GenerateBlindIndex(string input, string pepperSecret)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(pepperSecret);
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using var hmac = new HMACSHA256(keyBytes);
        byte[] hashBytes = hmac.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes);

    }

    private static byte[] DeriveKey(string passphrase, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passphrase))
        {
            Salt = salt,
            DegreeOfParallelism = Argon2DegreeOfParallelism,
            MemorySize = Argon2MemorySizeKb,
            Iterations = Argon2Iterations
        };
        return argon2.GetBytes(KeySizeBytes);
    }

}
