using StrategyDesignPattern.Contracts;
using StrategyDesignPattern.Models;

namespace StrategyDesignPattern.Strategies;

/// <summary>
/// CONCRETE STRATEGY — nothing needs transforming, so the file store copies the object
/// server-side without sending the bytes through the application.
/// </summary>
public sealed class PlainCopyStrategy(IFileStore store) : IFileArchiveStrategy
{
    public string Name => "plain copy";

    public async Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var written = await store.CopyAsync(file.SourcePath, destinationPath);

        return new ArchiveResult(Name, destinationPath, written, "copied server-side");
    }
}
