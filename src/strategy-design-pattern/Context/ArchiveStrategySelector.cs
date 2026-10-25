using StrategyDesignPattern.Contracts;
using StrategyDesignPattern.Models;
using StrategyDesignPattern.Strategies;

namespace StrategyDesignPattern.Context;

/// <summary>
/// The simple factory that replaces the original if / else-if / else. It is the only code
/// that knows the file shapes; the strategies below it know nothing about each other.
/// </summary>
public sealed class ArchiveStrategySelector(
    EncryptedUploadStrategy encryptedUpload,
    CompressedFileStrategy compressedFile,
    PlainCopyStrategy plainCopy)
{
    public IFileArchiveStrategy For(ArchivedFile file)
    {
        if (file.IsEncryptedUpload)
        {
            return encryptedUpload;
        }

        if (file.IsCompressed)
        {
            return compressedFile;
        }

        return plainCopy;
    }
}
