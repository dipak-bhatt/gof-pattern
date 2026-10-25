using StrategyDesignPattern.Models;

namespace StrategyDesignPattern.Contracts;

/// <summary>
/// STRATEGY — one interchangeable way to copy a stored file into the archive.
/// </summary>
/// <remarks>
/// The three implementations read from different places and transform the bytes differently,
/// but the caller uses the same method for all of them. Adding another way to obtain the
/// bytes means adding a class, not another branch in the archive loop.
/// </remarks>
public interface IFileArchiveStrategy
{
    /// <summary>A short name shown by the demo so the chosen algorithm is visible.</summary>
    string Name { get; }

    /// <summary>Copy <paramref name="file"/> to <paramref name="destinationPath"/>.</summary>
    Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
