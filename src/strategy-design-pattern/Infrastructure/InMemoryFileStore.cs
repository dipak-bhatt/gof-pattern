using StrategyDesignPattern.Contracts;

namespace StrategyDesignPattern.Infrastructure;

/// <summary>A small in-memory stand-in for object storage, with a log of the calls it receives.</summary>
public sealed class InMemoryFileStore : IFileStore
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);

    /// <summary>Every call made against the store, in order, so the demo can print them.</summary>
    public List<string> Actions { get; } = [];

    public void Seed(string path, byte[] content) => _files[path] = content;

    public byte[] Read(string path) => _files[path];

    public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        Actions.Add($"OpenReadAsync(\"{path}\")");
        return Task.FromResult<Stream>(new MemoryStream(_files[path], writable: false));
    }

    public async Task<long> WriteAsync(
        string path,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);

        _files[path] = copy.ToArray();
        Actions.Add($"WriteAsync(\"{path}\", {copy.Length} bytes)");
        return copy.Length;
    }

    public Task<long> CopyAsync(string sourcePath, string destinationPath)
    {
        _files[destinationPath] = _files[sourcePath];
        Actions.Add($"CopyAsync(\"{sourcePath}\" -> \"{destinationPath}\")");
        return Task.FromResult((long)_files[destinationPath].Length);
    }
}
