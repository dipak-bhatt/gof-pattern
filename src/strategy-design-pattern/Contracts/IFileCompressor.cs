namespace StrategyDesignPattern.Contracts;

/// <summary>Decompresses files that are stored compressed.</summary>
public interface IFileCompressor
{
    Task<Stream> DecompressAsync(Stream compressedStream, CancellationToken cancellationToken = default);
}
