namespace StrategyDesignPattern.Contracts;

/// <summary>Decrypts files that were written through the legacy encrypted upload path.</summary>
public interface IEncryptorDecryptor
{
    byte[] Decrypt(Stream source);
}
