using StrategyDesignPattern.Models;

namespace StrategyDesignPattern.Context;

/// <summary>
/// CONTEXT — the archive job. It asks the selector for a strategy and delegates to it.
/// It never tests <see cref="ArchivedFile.IsEncryptedUpload"/> or
/// <see cref="ArchivedFile.IsCompressed"/> itself.
/// </summary>
public sealed class FileArchiveJob(ArchiveStrategySelector selector)
{
    public async Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var strategy = selector.For(file);
        return await strategy.ArchiveAsync(file, destinationPath, cancellationToken);
    }
}
