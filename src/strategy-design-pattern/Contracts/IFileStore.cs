namespace StrategyDesignPattern.Contracts;

/// <summary>The small part of a file store the archive strategies need.</summary>
public interface IFileStore
{
    /// <summary>Open a stored file for reading.</summary>
    Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Write a stream to <paramref name="path"/> and return the number of bytes written.</summary>
    Task<long> WriteAsync(string path, Stream content, CancellationToken cancellationToken = default);

    /// <summary>Copy a file inside the store without reading it into the application.</summary>
    Task<long> CopyAsync(string sourcePath, string destinationPath);
}
