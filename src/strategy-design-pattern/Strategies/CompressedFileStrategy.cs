using StrategyDesignPattern.Contracts;
using StrategyDesignPattern.Models;

namespace StrategyDesignPattern.Strategies;

/// <summary>
/// CONCRETE STRATEGY — the file is stored compressed, so this strategy reads it, decompresses
/// it and writes the readable bytes to the archive.
/// </summary>
public sealed class CompressedFileStrategy(IFileStore store, IFileCompressor compressor)
    : IFileArchiveStrategy
{
    public string Name => "compressed file";

    public async Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        await using var source = await store.OpenReadAsync(file.SourcePath, cancellationToken);
        await using var decompressed = await compressor.DecompressAsync(source, cancellationToken);

        var written = await store.WriteAsync(destinationPath, decompressed, cancellationToken);

        return new ArchiveResult(Name, destinationPath, written, "decompressed");
    }
}
