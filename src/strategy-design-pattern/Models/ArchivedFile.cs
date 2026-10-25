namespace StrategyDesignPattern.Models;

/// <summary>
/// One file waiting to be copied into the archive. The two flags describe how the file is
/// stored today, and they are the only thing that decides which strategy is used.
/// </summary>
public sealed record ArchivedFile(
    string SourcePath,
    Guid AccountId,
    bool IsEncryptedUpload = false,
    bool IsCompressed = false);
