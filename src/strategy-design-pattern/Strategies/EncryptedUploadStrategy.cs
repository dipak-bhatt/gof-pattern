using StrategyDesignPattern.Contracts;
using StrategyDesignPattern.Models;

namespace StrategyDesignPattern.Strategies;

/// <summary>
/// CONCRETE STRATEGY — files written through the legacy upload path are stored encrypted under
/// <c>uploads/{accountId}/</c>, so this strategy reads there and decrypts the bytes.
/// </summary>
public sealed class EncryptedUploadStrategy(IFileStore store, IEncryptorDecryptor decryptor)
    : IFileArchiveStrategy
{
    public string Name => "encrypted upload";

    public async Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var uploadPath = $"uploads/{file.AccountId}/{Path.GetFileName(file.SourcePath)}";

        await using var source = await store.OpenReadAsync(uploadPath, cancellationToken);
        var decrypted = decryptor.Decrypt(source);

        await using var content = new MemoryStream(decrypted);
        var written = await store.WriteAsync(destinationPath, content, cancellationToken);

        return new ArchiveResult(Name, destinationPath, written, "decrypted");
    }
}
