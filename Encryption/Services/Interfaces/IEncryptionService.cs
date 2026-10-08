namespace Encryption.Services.Interfaces;

public record EncryptionResult(
    string CiphertextBase64,
    string IvBase64,
    string SaltBase64,
    string AuthTagBase64,
    string BlindIndexHash
    );


public interface IEncryptionService
{
    public EncryptionResult Encrypt(string plainUrl, string passphrase);

    public string Decrypt(
        string ciphertextBase64,
        string ivBase64,
        string saltBase64,
        string authTagBase64,
        string passphrase
    );
    
    public string GenerateBlindIndex(string input, string pepperSecret);

}
