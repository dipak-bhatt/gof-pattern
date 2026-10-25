using System.Text;
using StrategyDesignPattern.Contracts;

namespace StrategyDesignPattern.Infrastructure;

/// <summary>Stands in for the real decryptor: encrypted files start with <c>ENC:</c>.</summary>
public sealed class PrefixDecryptor : IEncryptorDecryptor
{
    private static readonly byte[] Prefix = "ENC:"u8.ToArray();

    public byte[] Decrypt(Stream source)
    {
        using var buffer = new MemoryStream();
        source.CopyTo(buffer);

        var bytes = buffer.ToArray();
        if (!bytes.AsSpan().StartsWith(Prefix))
        {
            throw new InvalidDataException("That upload is not encrypted.");
        }

        return bytes[Prefix.Length..];
    }

    public static byte[] Encrypt(string text) =>
        [.. Prefix, .. Encoding.UTF8.GetBytes(text)];
}
