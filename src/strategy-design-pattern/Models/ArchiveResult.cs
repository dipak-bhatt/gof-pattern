namespace StrategyDesignPattern.Models;

/// <summary>What one archive run did, whichever strategy performed it.</summary>
public sealed record ArchiveResult(
    string StrategyName,
    string DestinationPath,
    long BytesWritten,
    string Detail);
