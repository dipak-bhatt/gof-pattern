using System.IO.Compression;
using StrategyDesignPattern.Contracts;

namespace StrategyDesignPattern.Infrastructure;

/// <summary>GZip, in the same shape as the production compressor interface.</summary>
public sealed class GZipCompressor : IFileCompressor
{
    public async Task<Stream> DecompressAsync(
        Stream compressedStream,
        CancellationToken cancellationToken = default)
    {
        var output = new MemoryStream();

        await using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress, leaveOpen: true))
        {
            await gzip.CopyToAsync(output, cancellationToken);
        }

        output.Position = 0;
        return output;
    }

    public static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();

        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(data);
        }

        return output.ToArray();
    }
}
